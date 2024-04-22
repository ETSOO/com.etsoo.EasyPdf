using com.etsoo.EasyPdf.Objects;
using com.etsoo.EasyPdf.Support;
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
        /// Current line
        /// 当前行
        /// </summary>
        protected PdfBlockLine CurrentLine { get; private set; } = new(0);

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

        private RectangleF layout;
        private bool hasBorder;

        /// <summary>
        /// Set parent style
        /// 设置父样式
        /// </summary>
        /// <param name="parentStyle">Parent style</param>
        /// <param name="fontSize">Page default font size</param>
        public virtual void SetParentStyle(PdfStyle parentStyle, float fontSize)
        {
            Style.Parent = parentStyle;
        }

        /// <summary>
        /// New line action
        /// 新行动作
        /// </summary>
        /// <param name="page">Current page</param>
        /// <param name="writer">Current writer</param>
        /// <param name="style">Current style</param>
        /// <param name="rect">Current rectangle</param>
        /// <param name="line">Current completed line</param>
        /// <param name="newLine">New line</param>
        /// <returns></returns>
        protected virtual async Task NewLineActionAsync(IPdfPage page, PdfWriter writer, PdfStyle style, RectangleF rect, PdfBlockLine line, PdfBlockLine? newLine)
        {
            var bgcolor = style.BackgroundColor;
            if (line.Chunks.Length > 0 && (bgcolor.HasValue || hasBorder))
            {
                // Start point
                var startPoint = line.Chunks.First().StartPoint;

                var x = layout.X;
                var width = layout.Width;
                float y, height;

                if (line.First)
                {
                    // First line
                    height = startPoint.Y - layout.Y + line.Height;
                    y = layout.Y + height;
                }
                else if (newLine == null)
                {
                    // Last line
                    height = line.Height + style.Padding?.Bottom ?? 0;
                    y = startPoint.Y + height;
                }
                else
                {
                    // Other lines
                    height = line.Height;
                    y = startPoint.Y + height;
                }

                // Line rectangle
                var gPoint = page.CalculatePoint(new Vector2(x, y));
                var lineRect = new RectangleF(gPoint.X, gPoint.Y, width, height);

                // Draw background color
                if (bgcolor.HasValue)
                {
                    //await page.WriteBackgroundAsync(bgcolor.Value, lineRect);
                }

                // Draw border
                if (hasBorder)
                {
                    var border = style.Border!.DeepClone();

                    if (line.Index == 0)
                    {
                        border.Bottom.Width = 0;
                    }
                    else if (newLine == null)
                    {
                        border.Top.Width = 0;
                    }
                    else
                    {
                        border.Bottom.Width = 0;
                        border.Top.Width = 0;
                    }

                    await page.WriteBorderAsync(border, lineRect);
                }
            }

            // Update current line reference
            // newLine is null means the line is the last one
            if (newLine != null)
                CurrentLine = newLine;
        }

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

            // Adjust overlapping
            if (style.Position != PdfPosition.Absolute)
            {
                writer.AdjustMargin(style.Margin);
            }

            // Rectangle
            var (layout, rect) = style.GetRectangle(page.ContentRect.Size, page.CurrentPoint);
            this.layout = layout;
            hasBorder = style.Border?.HasBorder ?? false;

            // Rotate
            var angle = style.Rotate;
            if (angle != 0)
            {
                var height = style.Height ?? style.FontSize ?? writer.CurrentFont?.LineHeight ?? 0;
                var adjust = height * Math.Sin(angle) / 2;
                var anglePoint = new Vector2(rect.X, rect.Y);
                BasePoint = page.CalculatePoint(anglePoint);
                var half = rect.Width / 2;
                var bytes = PdfOperator.Rotate(angle, (BasePoint.X + half - half * Math.Cos(angle) + adjust).ToSingle(), (BasePoint.Y + half * Math.Sin(angle)).ToSingle());
                await page.Stream.WriteAsync(bytes);
            }

            // Opacity
            var opacity = style.Opacity;
            if (opacity < 1)
            {
                await writer.DefineOpacityAsync(opacity, true);
            }

            // Current point
            var point = style.Position == PdfPosition.Absolute ? new PdfPoint
            {
                X = rect.X,
                Y = rect.Y
            } : page.CurrentPoint;

            // Write
            await WriteInnerAsync(page, writer, style, rect, point);

            // Store graphics state
            await page.RestoreStateAsync();

            // Adjust
            AdjustBottom(point, style);

            // Update render status
            Rendered = true;
        }

        /// <summary>
        /// Adjust bottom space
        /// 调整底部空间
        /// </summary>
        /// <param name="point">Current point</param>
        /// <param name="style">Style</param>
        protected virtual void AdjustBottom(PdfPoint point, PdfStyle style)
        {
            var paddingBottom = style.Padding?.Bottom ?? 0;
            var marginBottom = style.Margin?.Bottom ?? 0 + style.Border?.Bottom.Width ?? 0;
            point.Y += paddingBottom + marginBottom;
        }

        /// <summary>
        /// Write inner content
        /// 输出内部内容
        /// </summary>
        /// <param name="page">Current page</param>
        /// <param name="writer">Current writer</param>
        /// <param name="style">Current style</param>
        /// <param name="rect">Current rectangle</param>
        /// <param name="point">Current point</param>
        /// <returns>Task</returns>
        protected abstract ValueTask WriteInnerAsync(IPdfPage page, PdfWriter writer, PdfStyle style, RectangleF rect, PdfPoint point);
    }
}
