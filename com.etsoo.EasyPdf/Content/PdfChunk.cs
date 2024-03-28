using com.etsoo.EasyPdf.Support;
using System.Drawing;

namespace com.etsoo.EasyPdf.Content
{
    /// <summary>
    /// PDF chunk (inline element)
    /// PDF 块（行内元素）
    /// </summary>
    public abstract class PdfChunk : IPdfElement
    {
        /// <summary>
        /// Style
        /// 样式
        /// </summary>
        public PdfStyle Style { get; } = new PdfStyle();

        /// <summary>
        /// Write chunk
        /// 输出块
        /// </summary>
        /// <param name="writer">Writer</param>
        /// <param name="rect">Drawing rectangle</param>
        /// <param name="point">Current point</param>
        /// <param name="line">Current line</param>
        /// <param name="newLineAction">New line action</param>
        /// <returns>Need new page or not</returns>
        public abstract Task<bool> WriteAsync(IPdfWriter writer, RectangleF rect, PdfPoint point, PdfBlockLine line, Func<PdfBlockLine, PdfBlockLine, Task> newLineAction);
    }
}
