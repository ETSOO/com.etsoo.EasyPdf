using com.etsoo.EasyPdf.Objects;
using System.Numerics;

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
        private bool firstLine = true;

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
        /// <param name="newline">Is new line</param>
        public PdfChunk Add(ReadOnlySpan<char> content, bool newline = false)
        {
            if (Rendered)
                throw new InvalidOperationException("The block has been rendered.");

            var chunk = new PdfChunk(content) { NewLine = newline };
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

            // Save graphics state
            await page.SaveStateAsync();

            // Begin text
            await page.BeginTextAsync();

            // Block level font
            await writer.WriteFontAsync(page.Stream, style, true);

            var c = 0;
            for (; c < len; c++)
            {
                var chunk = chunks[c];
                if (await chunk.WriteAsync(writer, rect, page.CurrentPoint, currentLine, async (line, newLine) =>
                {
                    // Output previous line
                    await WriteLineAsync(line, page, rect.Width);

                    // Update current line reference
                    currentLine = newLine;
                }))
                {
                    break;
                }
            }

            if (!currentLine.Rendered)
            {
                await WriteLineAsync(currentLine, page, rect.Width);

                // Move to the beginning of the next line
                page.CurrentPoint.X = 0;
            }

            // End text
            await page.EndTextAsync();

            // Restore graphics state
            await page.RestoreStateAsync();

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
            if (firstLine)
            {
                // For block element, move to next line from previous point
                await page.MoveToAsync(new Vector2(0, page.CurrentPoint.Y), line.Height);
                firstLine = false;
            }

            if (line.FullWidth && line.Width != width)
            {
                if (line.Words < 0)
                {
                    // Adjust by word spacing
                    var tw = (width - line.Width) / line.Words;
                    await page.Stream.WriteAsync(PdfOperator.Tc(0));
                    await page.Stream.WriteAsync(PdfOperator.Tw(tw));
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

            foreach (var chunk in line.Chunks)
            {
                if (chunk.Operators.Count > 0)
                {
                    await page.Stream.WriteAsync(chunk.Operators.ToArray());
                }

                if (chunk.Chars.Count > 0)
                {
                    await chunk.Font.WriteAsync(page.Stream, chunk.Chars);
                    await page.Stream.WriteAsync(PdfOperator.SQ);
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
