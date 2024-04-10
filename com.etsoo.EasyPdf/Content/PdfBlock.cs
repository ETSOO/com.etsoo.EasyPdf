using com.etsoo.EasyPdf.Objects;
using System.Text;

namespace com.etsoo.EasyPdf.Content
{
    /// <summary>
    /// PDF block, behaves like HTML DIV
    /// PDF 内容块
    /// </summary>
    public class PdfBlock : IPdfElement
    {
        /// <summary>
        /// All chunks
        /// 全部块
        /// </summary>
        private readonly List<PdfChunk> chunks = [];

        /// <summary>
        /// Style
        /// 样式
        /// </summary>
        public PdfStyle Style { get; private set; } = new PdfStyle();

        /// <summary>
        /// Is rendered
        /// 是否已渲染
        /// </summary>
        public bool Rendered { get; private set; }

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

        public virtual async ValueTask WriteAsync(IPdfPage page, IPdfWriter writer)
        {
            // No dynamic content, if rendered, then totally done
            if (Rendered)
                throw new InvalidOperationException("The block has been rendered.");

            var len = chunks.Count;
            if (len == 0)
            {
                Rendered = true;
                return;
            }

            // Computed style
            var style = Style.GetComputedStyle();

            // Rectangle
            var rect = style.GetRectangle(page.ContentRect.Size, page.CurrentPoint.ToVector2());

            var c = 0;
            PdfChunk chunk = chunks[0];
            for (; c < len; c++)
            {
                chunk = chunks[c];

                var completed = await chunk.WriteAsync(writer, rect, page.CurrentPoint, currentLine, async (line, newLine) =>
                {
                    // Output previous line
                    await WriteLineAsync(line, page, rect.Width, style);

                    // Update current line reference
                    currentLine = newLine;
                });

                if (completed)
                {
                    break;
                }
            }

            if (!currentLine.Rendered)
            {
                await WriteLineAsync(currentLine, page, rect.Width, style);

                // Move to the beginning of the next line
                page.CurrentPoint.X = 0;
                page.CurrentPoint.Y += currentLine.Height;
            }

            Rendered = true;

            if (c < len)
            {
                var newBlock = new PdfBlock
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
        }

        private async Task WriteLineAsync(PdfBlockLine line, IPdfPage page, float width, PdfStyle style)
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

            for (var c = 0; c < chunkCount; c++)
            {
                var chunk = line.Chunks[c];

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

            line.Rendered = true;
        }
    }
}
