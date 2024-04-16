using com.etsoo.EasyPdf.Objects;
using System.Drawing;
using System.Text;

namespace com.etsoo.EasyPdf.Content
{
    /// <summary>
    /// PDF rich block
    /// PDF 富文本块
    /// </summary>
    public class PdfRichBlock : PdfBlock
    {
        /// <summary>
        /// All chunks
        /// 全部块
        /// </summary>
        private readonly List<PdfChunk> chunks = [];

        private PdfBlockLine currentLine = new();

        /// <summary>
        /// Add chunk
        /// 添加块
        /// </summary>
        /// <param name="chunk">Chunk</param>
        public void Add(PdfChunk chunk)
        {
            var last = chunks.LastOrDefault();
            if (last != null)
            {
                last.NextSibling = chunk;
                chunk.PreviousSibling = last;
            }

            chunks.Add(chunk);
            chunk.Owner = this;
            chunk.Style.Parent = Style;
        }

        /// <summary>
        /// Add text content
        /// 添加文本内容
        /// </summary>
        /// <param name="content">Content</param>
        public PdfTextChunk Add(ReadOnlySpan<char> content)
        {
            if (Rendered)
                throw new InvalidOperationException("The block has been rendered.");

            var chunk = new PdfTextChunk(content);
            Add(chunk);
            return chunk;
        }

        /// <summary>
        /// New line action
        /// 新行动作
        /// </summary>
        /// <param name="page">Current page</param>
        /// <param name="writer">Current writer</param>
        /// <param name="style">Current style</param>
        /// <param name="rect">Current rectangle</param>
        /// <param name="line">Current completed line</param>
        /// <param name="newLine">New line</param>
        /// <returns></returns>
        protected virtual async Task NewLineActionAsync(IPdfPage page, PdfWriter writer, PdfStyle style, RectangleF rect, PdfBlockLine line, PdfBlockLine? newLine)
        {
            // Output previous line
            await WriteLineAsync(line, page, writer, rect.Width, style);

            // Update current line reference
            // newLine is null means the line is the last one
            if (newLine != null)
                currentLine = newLine;
        }

        protected override async ValueTask WriteInnerAsync(IPdfPage page, PdfWriter writer, PdfStyle style, RectangleF rect)
        {
            var len = chunks.Count;
            if (len == 0)
            {
                return;
            }

            // Margin
            var margin = style.Margin;

            // New line action
            Task action(PdfBlockLine line, PdfBlockLine? newLine) => NewLineActionAsync(page, writer, style, rect, line, newLine);

            var c = 0;
            PdfChunk chunk = chunks[0];
            for (; c < len; c++)
            {
                chunk = chunks[c];

                var completed = await chunk.WriteAsync(writer, rect, page.CurrentPoint, currentLine, action);

                if (completed)
                {
                    break;
                }
            }

            if (!currentLine.Rendered)
            {
                await action(currentLine, null);

                // Move to the beginning of the next line
                page.CurrentPoint.X = 0;
                page.CurrentPoint.Y += currentLine.Height;
            }

            if (c < len)
            {
                var newBlock = new PdfRichBlock
                {
                    Style = Style
                };

                while (c < len)
                {
                    newBlock.Add(chunks[c]);
                    c++;
                }

                await writer.NewPageAsync();
                await writer.WriteAsync(newBlock);
            }
            else
            {
                // Move the margin bottom
                page.CurrentPoint.Y += margin.Bottom;
            }
        }

        private async Task WriteLineAsync(PdfBlockLine line, IPdfPage page, PdfWriter writer, float width, PdfStyle style)
        {
            var chunkCount = line.Chunks.Length;
            if (chunkCount == 0)
            {
                return;
            }

            // Text align rendering
            float wordSpacing = 0;
            float letterSpacing = 0;

            if (line.FullWidth && line.Width != width && style.TextAlign == PdfTextAlign.Justify)
            {
                if (line.Spaces > 0)
                {
                    // Adjust by word spacing
                    // Only support single-byte encoded characters
                    // For multi-byte encoded characters, no effect as the space character mapping may not be the same
                    wordSpacing = (width - line.Width) / line.Spaces;
                }
                else
                {
                    // Adjust by letter spacing
                    letterSpacing = (width - line.Width) / line.Chunks.Sum(c => c.Chars.Count);
                }

                // Update line width
                line.Width = width;
            }
            else if (line.Width != width && style.TextAlign == PdfTextAlign.End)
            {
                var adjust = width - line.Width;
                for (var c = 0; c < chunkCount; c++)
                {
                    line.Chunks[c].AdjustStartPoint(adjust);
                }
            }
            else if (line.Width != width && style.TextAlign == PdfTextAlign.Center)
            {
                var adjust = (width - line.Width) / 2;
                for (var c = 0; c < chunkCount; c++)
                {
                    line.Chunks[c].AdjustStartPoint(adjust);
                }
            }
            else
            {
                await page.Stream.WriteAsync(PdfOperator.Tc(0));
                // await page.Stream.WriteAsync(PdfOperator.Tw(0));
            }

            // Line height
            var lineHeight = line.Height;
            var firstHeight = line.Chunks[0].Font.LineHeight;

            var chunkIndex = 0;
            PdfChunk? chunkObj = null;

            for (var c = 0; c < chunkCount; c++)
            {
                var chunk = line.Chunks[c];

                if (chunkObj == null)
                {
                    // Initialize
                    chunkObj = chunk.Owner;
                }
                else if (chunkObj != chunk.Owner)
                {
                    // New chunk object
                    await chunkObj.NewLineActionAsync(line, chunkIndex, page, writer, width, style);
                    chunkObj = chunk.Owner;
                    chunkIndex = c;
                }

                if (!chunk.IsSequence)
                {
                    await chunk.Owner.CalculatePositionAsync(page, line, chunk);
                }

                if (chunk.Operators.Count > 0)
                {
                    foreach (var op in chunk.Operators)
                    {
                        await page.Stream.WriteAsync(op);
                    }
                }

                var total = chunk.Chars.Count;
                if (total > 0)
                {
                    // Chunk spacing
                    var chunkLetterSpacing = chunk.Style.LetterSpacing.GetValueOrDefault().PxToPt() + letterSpacing;
                    var chunkWordSpacing = chunk.Style.WordSpacing.GetValueOrDefault().PxToPt() + wordSpacing;

                    await page.Stream.WriteAsync(PdfOperator.Tc(chunkLetterSpacing));

                    if (chunkWordSpacing == 0 || !chunk.BlankChar.HasValue)
                    {
                        await chunk.Font.WriteAsync(page.Stream, chunk.Chars);
                        await page.Stream.WriteAsync(PdfOperator.SQ);
                    }
                    else
                    {
                        // New line
                        await page.Stream.WriteAsync(PdfOperator.T42);

                        // Array start
                        page.Stream.WriteByte(PdfConstants.LeftSquareBracketByte);

                        // Split by space char
                        var lastPos = 0;
                        var pos = chunk.Chars.IndexOf(chunk.BlankChar.Value, lastPos);
                        var chunkWords = 0;
                        while (pos != -1)
                        {
                            pos++;
                            var chars = chunk.Chars.GetRange(lastPos, pos - lastPos);

                            if (chunkWords > 0)
                            {
                                // Space char
                                page.Stream.WriteByte(PdfConstants.SpaceByte);
                            }

                            await chunk.Font.WriteAsync(page.Stream, chars);

                            // Space char
                            page.Stream.WriteByte(PdfConstants.SpaceByte);

                            // Kerning
                            await page.Stream.WriteAsync(Encoding.UTF8.GetBytes($"{-chunk.Font.LocalSizeToFUnit(chunkWordSpacing)}"));

                            chunkWords++;
                            lastPos = pos;
                            pos = chunk.Chars.IndexOf(chunk.BlankChar.Value, lastPos);
                        }

                        if (lastPos < total)
                        {
                            // Space char
                            page.Stream.WriteByte(PdfConstants.SpaceByte);

                            await chunk.Font.WriteAsync(page.Stream, chunk.Chars.GetRange(lastPos, total - lastPos));
                        }

                        // Adjust next sibling chunks
                        var adjust = chunkWords * chunkWordSpacing;
                        for (var n = c + 1; n < chunkCount; n++)
                        {
                            line.Chunks[n].AdjustStartPoint(adjust);
                        }

                        // Array end
                        page.Stream.WriteByte(PdfConstants.RightSquareBracketByte);

                        // TJ for kerning
                        await page.Stream.WriteAsync(PdfOperator.TJ);
                    }
                }

                if (chunk.EndOperators.Count > 0)
                {
                    foreach (var op in chunk.EndOperators)
                    {
                        await page.Stream.WriteAsync(op);
                    }
                }
            }

            if (chunkObj != null)
            {
                // Last item
                await chunkObj.NewLineActionAsync(line, chunkIndex, page, writer, width, style);
            }

            line.Rendered = true;
        }
    }
}
