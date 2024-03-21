namespace com.etsoo.EasyPdf.Content
{
    /// <summary>
    /// PDF header and footer
    /// PDF 页眉和页脚
    /// </summary>
    /// <remarks>
    /// Constructor
    /// 构造函数
    /// </remarks>
    /// <param name="isHeader">Is header</param>
    public class PdfHeaderFooter(bool isHeader) : IPdfElement
    {
        /// <summary>
        /// Height
        /// 高度
        /// </summary>
        public int Height { get; set; }

        /// <summary>
        /// Is header
        /// 是否为页眉
        /// </summary>
        public bool IsHeader { get; } = isHeader;

        /// <summary>
        /// Style
        /// 样式
        /// </summary>
        public PdfStyle Style { get; } = new PdfStyle();
    }
}