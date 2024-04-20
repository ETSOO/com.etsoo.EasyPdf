using com.etsoo.EasyPdf.Objects;
using com.etsoo.EasyPdf.Support;
using System.Drawing;
using System.Numerics;

namespace com.etsoo.EasyPdf.Content
{
    /// <summary>
    /// PDF chunk type
    /// PDF 块类型
    /// </summary>
    public enum PdfChunkType
    {
        HR,
        Link,
        Image,
        Subscript,
        Superscript,
        Text
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
        /// Owner
        /// 所有者
        /// </summary>
        internal PdfBlock? Owner { get; set; }

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
        /// End point
        /// 结束点
        /// </summary>
        public Vector2 EndPoint { get; protected set; }

        /// <summary>
        /// Start point
        /// 开始点
        /// </summary>
        public Vector2 StartPoint { get; protected set; }

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
        public virtual async Task<bool> WriteAsync(PdfWriter writer, RectangleF rect, PdfPoint point, PdfBlockLine line, Func<PdfBlockLine, PdfBlockLine?, Task> newLineAction)
        {
            // Computed style
            var style = Style.GetComputedStyle();

            // Opacity
            var opacity = style.Opacity;
            if (opacity < 1)
            {
                await writer.DefineOpacityAsync(opacity, true);
            }

            // Margin left
            var marginLeft = (style.Margin?.Left ?? 0).PxToPt();

            // Margin right
            var marginRight = (style.Margin?.Right ?? 0).PxToPt();

            // Start point
            StartPoint = point.ToVector2();

            // Margin left
            point.X += marginLeft;
            line.Width += marginLeft;

            // Inner rendering
            var newPage = await WriteInnerAsync(writer, style, rect, point, line, newLineAction);

            // Margin right
            point.X += marginRight;
            line.Width += marginRight;

            // End point
            EndPoint = point.ToVector2();

            // Return
            return newPage;
        }

        /// <summary>
        /// Write inner chunk
        /// 输出内部块
        /// </summary>
        /// <param name="writer">Writer</param>
        /// <param name="style">Calculated style</param>
        /// <param name="rect">Drawing rectangle</param>
        /// <param name="point">Current point</param>
        /// <param name="line">Current line</param>
        /// <param name="newLineAction">New line action</param>
        /// <returns>Need new page or not</returns>
        public abstract Task<bool> WriteInnerAsync(PdfWriter writer, PdfStyle style, RectangleF rect, PdfPoint point, PdfBlockLine line, Func<PdfBlockLine, PdfBlockLine?, Task> newLineAction);

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

        /// <summary>
        /// New line action
        /// 新行动作
        /// </summary>
        /// <param name="line">Current line</param>
        /// <param name="chunkIndex">Chunk start index</param>
        /// <param name="page">Current page</param>
        /// <param name="writer">Current writer</param>
        /// <param name="width">Width</param>
        /// <param name="style">Current style</param>
        /// <returns>Task</returns>
        public virtual Task NewLineActionAsync(PdfBlockLine line, int chunkIndex, IPdfPage page, PdfWriter writer, float width, PdfStyle style)
        {
            return Task.CompletedTask;
        }
    }
}
