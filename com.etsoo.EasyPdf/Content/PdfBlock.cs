using com.etsoo.EasyPdf.Fonts;
using com.etsoo.EasyPdf.Objects;
using System.Numerics;
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
            for (; c < len; c++)
            {
                var chunk = chunks[c];

                var completed = await chunk.WriteAsync(writer, rect, page.CurrentPoint, currentLine, async (line, newLine) =>
                {
                    // Output previous line
                    await WriteLineAsync(line, page, rect.Width);

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
                await WriteLineAsync(currentLine, page, rect.Width);

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

        private async Task WriteLineAsync(PdfBlockLine line, IPdfPage page, float width)
        {
            var chunkCount = line.Chunks.Count;
            if (chunkCount == 0)
            {
                return;
            }

            float wordSpacing = 0;

            if (line.FullWidth && line.Width != width)
            {
                if (line.Words > 1)
                {
                    // Adjust by word spacing
                    // Only support single-byte encoded characters
                    // For multi-byte encoded characters, no effect as the space character mapping may not be the same
                    wordSpacing = (width - line.Width) / line.Words;
                    await page.Stream.WriteAsync(PdfOperator.Tc(0));
                    await page.Stream.WriteAsync(PdfOperator.Tw(0));
                }
                else
                {
                    // Adjust by letter spacing
                    var tc = (width - line.Width) / line.Chunks.Sum(c => c.Chars.Count);
                    await page.Stream.WriteAsync(PdfOperator.Tc(tc));
                    await page.Stream.WriteAsync(PdfOperator.Tw(0));
                }
            }
            else
            {
                await page.Stream.WriteAsync(PdfOperator.Tc(0));
                await page.Stream.WriteAsync(PdfOperator.Tw(0));
            }

            var lineHeight = line.Height;
            var firstHeight = line.Chunks[0].Height;

            for (var c = 0; c < chunkCount; c++)
            {
                var chunk = line.Chunks[c];
                if (chunk.Operators.Count > 0)
                {
                    await page.Stream.WriteAsync(chunk.Operators.ToArray());
                }

                if (chunk.StartPoint.HasValue)
                {
                    var adjust = lineHeight - chunk.Height;
                    var diff = chunk.Height - firstHeight;
                    var isItalic = chunk.FontStyle.HasFlag(PdfFontStyle.Italic);

                    var point = page.CalculatePoint(chunk.StartPoint.Value);
                    var x = point.X;
                    var y = point.Y - adjust + diff * 0.1f;
                    if (isItalic)
                    {
                        x += PdfFontUtils.GetItalicSize(chunk.Font.Size) + diff * 0.08f;
                    }

                    var angle = isItalic ? PdfFontUtils.ItalicAngle : 0;
                    var pointBytes = PdfOperator.Tm(1, 0, angle, 1, x, y, true);
                    await page.Stream.WriteAsync(pointBytes);
                }

                var total = chunk.Chars.Count;
                if (total > 0)
                {
                    if (wordSpacing == 0 || !chunk.BlankChar.HasValue)
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
                            await page.Stream.WriteAsync(Encoding.UTF8.GetBytes($"-{wordSpacing * 72}"));

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
                        for (var n = c + 1; n < chunkCount; n++)
                        {
                            var nextChunk = line.Chunks[n];
                            if (nextChunk.StartPoint.HasValue)
                            {
                                var p = nextChunk.StartPoint.Value;
                                nextChunk.StartPoint = new Vector2(p.X + chunkWords * wordSpacing, p.Y);
                            }
                        }

                        // Array end
                        page.Stream.WriteByte(PdfConstants.RightSquareBracketByte);

                        // TJ for kerning
                        await page.Stream.WriteAsync(PdfOperator.TJ);
                    }
                }

                if (chunk.EndOperators.Count > 0)
                {
                    await page.Stream.WriteAsync(chunk.EndOperators.ToArray());
                }
            }

            line.Rendered = true;
        }
    }
}
