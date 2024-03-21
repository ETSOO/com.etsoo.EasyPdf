using com.etsoo.EasyPdf.Fonts;
using com.etsoo.EasyPdf.Objects;

namespace com.etsoo.EasyPdf.Content
{
    /// <summary>
    /// PDF chunk (inline element), behaves like HTML SPAN
    /// PDF 块（行内元素）
    /// </summary>
    public class PdfChunk : IPdfElement
    {
        /// <summary>
        /// Content
        /// 内容
        /// </summary>
        public ReadOnlyMemory<char> Content { get; init; }

        /// <summary>
        /// Is new line
        /// 是否为新行
        /// </summary>
        public bool NewLine { get; set; }

        /// <summary>
        /// Style
        /// 样式
        /// </summary>
        public PdfStyle Style { get; } = new PdfStyle();

        /// <summary>
        /// Constructor
        /// 构造函数
        /// </summary>
        /// <param name="content">Content</param>
        public PdfChunk(ReadOnlySpan<char> content)
        {
            Memory<char> cache = new char[content.Length];
            content.CopyTo(cache.Span);
            Content = cache;
        }

        public async Task WriteAsync(IPdfPage page, IPdfWriter writer)
        {
            // Computed style
            var style = Style.GetComputedStyle();

            // Save graphics state
            await page.SaveStateAsync();

            // Write font
            var font = await writer.WriteFontAsync(page.Stream, style);
            await font.SetStyleAsync(page.Stream);

            // Write color
            await page.SetFontColor(style.Color);

            // Superscript && Subscript
            if (style.TextStyle == PdfTextStyle.SuperScript || style.TextStyle == PdfTextStyle.SubScript)
            {

            }

            // Content
            await font.WriteAsync(page.Stream, this);

            // Restore graphics state
            await page.RestoreStateAsync();
        }
    }
}
