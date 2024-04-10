using com.etsoo.EasyPdf.Objects;
using com.etsoo.EasyPdf.Support;
using System.Drawing;

namespace com.etsoo.EasyPdf.Content
{
    /// <summary>
    /// PDF chunk type
    /// PDF 块类型
    /// </summary>
    public enum PdfChunkType
    {
        Text,
        Superscript,
        Subscript
    }

    /// <summary>
    /// PDF chunk (inline element)
    /// PDF 块（行内元素）
    /// </summary>
    public abstract class PdfChunk : IPdfElement
    {
        /// <summary>
        /// Line height
        /// 行高
        /// </summary>
        public float? LineHeight { get; set; }

        /// <summary>
        /// Next sibling chunk
        /// 下一个兄弟块
        /// </summary>
        internal PdfChunk? NextSibling { get; set; }

        /// <summary>
        /// Previous sibling chunk
        /// 上一个兄弟块
        /// </summary>
        internal PdfChunk? PreviousSibling { get; set; }

        /// <summary>
        /// Style
        /// 样式
        /// </summary>
        public PdfStyle Style { get; } = new PdfStyle();

        /// <summary>
        /// PDF chunk type
        /// PDF 块类型
        /// </summary>
        public PdfChunkType Type { get; }

        /// <summary>
        /// Constructor
        /// 构造函数
        /// </summary>
        /// <param name="type">Chunk type</param>
        public PdfChunk(PdfChunkType type)
        {
            Type = type;
        }

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

        /// <summary>
        /// Calculate position
        /// 计算位置
        /// </summary>
        /// <param name="page">Current page</param>
        /// <param name="line">Current line</param>
        /// <param name="chunk">Current chunk</param>
        /// <returns>Task</returns>
        public virtual Task CalculatePositionAsync(IPdfPage page, PdfBlockLine line, PdfBlockLineChunk chunk)
        {
            var point = page.CalculatePoint(chunk.StartPoint);
            var pointBytes = PdfOperator.Td(point);
            chunk.InsertAfter(pointBytes, PdfOperator.q);
            return Task.CompletedTask;
        }
    }
}
