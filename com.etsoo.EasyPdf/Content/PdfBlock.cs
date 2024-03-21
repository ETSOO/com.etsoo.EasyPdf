using com.etsoo.EasyPdf.Objects;

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
        public List<PdfChunk> Chunks { get; } = [];

        /// <summary>
        /// Style
        /// 样式
        /// </summary>
        public PdfStyle Style { get; } = new PdfStyle();

        /// <summary>
        /// Is rendered
        /// 是否已渲染
        /// </summary>
        public bool Rendered { get; private set; }

        /// <summary>
        /// Add chunk
        /// 添加块
        /// </summary>
        /// <param name="chunk">Chunk</param>
        public void Add(PdfChunk chunk)
        {
            Chunks.Add(chunk);
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

        public virtual async Task<bool> WriteAsync(IPdfPage page, IPdfWriter writer)
        {
            if (Rendered)
                throw new InvalidOperationException("The block has been rendered.");

            for (var c = 0; c < Chunks.Count; c++)
            {
                var chunk = Chunks[c];
                await chunk.WriteAsync(page, writer);
            }

            // It's a block element, move to start of next text line
            await page.Stream.WriteAsync(PdfOperator.T42);

            Rendered = true;

            return false;
        }
    }
}
