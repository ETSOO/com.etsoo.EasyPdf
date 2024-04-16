using com.etsoo.EasyPdf.Objects;
using System.Drawing;
using System.Numerics;

namespace com.etsoo.EasyPdf.Content
{
    /// <summary>
    /// PDF block
    /// PDF 内容块
    /// </summary>
    public abstract class PdfBlock : IPdfElement
    {
        /// <summary>
        /// Base point
        /// 根点
        /// </summary>
        public Vector2 BasePoint { get; set; }

        /// <summary>
        /// Style
        /// 样式
        /// </summary>
        public PdfStyle Style { get; set; } = new PdfStyle();

        /// <summary>
        /// Is rendered
        /// 是否已渲染
        /// </summary>
        public bool Rendered { get; protected set; }

        /// <summary>
        /// Write to PDF
        /// 输出到 PDF
        /// </summary>
        /// <param name="page">Current page</param>
        /// <param name="writer">Current writer</param>
        /// <returns>Task</returns>
        public virtual async ValueTask WriteAsync(IPdfPage page, PdfWriter writer)
        {
            // No dynamic content, if rendered, then totally done
            if (Rendered)
                throw new InvalidOperationException("The block has been rendered.");

            // Save graphics state
            await page.SaveStateAsync();

            // Computed style
            var style = Style.GetComputedStyle();

            // Rectangle
            var rect = style.GetRectangle(page.ContentRect.Size, page.CurrentPoint);

            // Border and background
            //await page.WriteBorderAndBackgroundAsync(style, rect);

            // Rotate
            var angle = style.Rotate;
            if (angle != 0)
            {
                var point = new Vector2(rect.X, rect.Y);
                BasePoint = page.CalculatePoint(point);
                var half = rect.Width / 2;
                var bytes = PdfOperator.Rotate(angle, (BasePoint.X + half - half * Math.Cos(angle)).ToSingle(), (BasePoint.Y + half * Math.Sin(angle)).ToSingle());
                await page.Stream.WriteAsync(bytes);
            }

            // Opacity
            var opacity = style.Opacity;
            if (opacity < 1)
            {
                await writer.DefineOpacityAsync(opacity, true);
            }

            // Write
            await WriteInnerAsync(page, writer, style, rect);

            // Store graphics state
            await page.RestoreStateAsync();

            // Update render status
            Rendered = true;
        }

        protected abstract ValueTask WriteInnerAsync(IPdfPage page, PdfWriter writer, PdfStyle style, RectangleF rect);
    }
}
