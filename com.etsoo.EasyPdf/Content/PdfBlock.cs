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
        /// Start point
        /// 开始点
        /// </summary>
        public Vector2 StartPoint;

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
        /// Bottom adjust
        /// 底部调整
        /// </summary>
        protected float BottomAdjust { get; private set; }

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
        /// <param name="point">Current point</param>
        /// <param name="line">Current completed line</param>
        /// <param name="newLine">New line</param>
        /// <returns></returns>
        protected virtual async Task NewLineActionAsync(IPdfPage page, PdfWriter writer, PdfStyle style, RectangleF rect, PdfPoint point, PdfBlockLine line, PdfBlockLine? newLine)
        {
            var bgcolor = style.BackgroundColor;
            if (bgcolor.HasValue)
            {
                var adjust = PdfTextChunk.LineHeightAdjust + 1;

                // Start point
                var startPoint = line.Chunks.FirstOrDefault()?.StartPoint ?? StartPoint;

                var x = layout.X;
                var width = layout.Width;
                var lineHeight = line.Height;
                float y, height;

                if (line.First)
                {
                    // First line
                    height = startPoint.Y - layout.Y + lineHeight;

                    if (newLine == null)
                    {
                        // Also the last line
                        height += BottomAdjust;
                    }
                    else
                    {
                        height += adjust;
                    }

                    y = layout.Y + height;
                }
                else if (newLine == null)
                {
                    // Last line
                    height = lineHeight + BottomAdjust;
                    y = startPoint.Y + adjust + height;
                }
                else
                {
                    // Other lines
                    height = lineHeight;
                    y = startPoint.Y + adjust + height;
                }

                // Line rectangle
                var gPoint = page.CalculatePoint(new Vector2(x, y));
                var lineRect = new RectangleF(gPoint.X, gPoint.Y, width, height);

                // Draw background color
                await page.WriteBackgroundAsync(bgcolor.Value, lineRect);
            }

            // Border
            if (hasBorder && newLine == null)
            {
                var adjust = line.Chunks.Length == 0 ? 0 : PdfTextChunk.LineHeightAdjust + 1;

                // Start point
                var startPoint = line.Chunks.FirstOrDefault()?.StartPoint ?? StartPoint;

                var width = layout.Width;
                var height = startPoint.Y - layout.Y + line.Height + BottomAdjust + adjust;
                var x = layout.X;
                var y = layout.Y + height;

                // Line rectangle
                var gPoint = page.CalculatePoint(new Vector2(x, y));
                var lineRect = new RectangleF(gPoint.X, gPoint.Y, width, height);

                // Draw border
                await page.WriteBorderAsync(style.Border!, lineRect);
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

            // Back to most left
            page.CurrentPoint.X = 0;

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
            var container = style.Position == PdfPosition.Absolute ? page.PageFullRect : page.ContentRect;
            var (layout, rect) = style.GetRectangle(container, page);
            this.layout = layout;

            BottomAdjust = layout.Bottom - rect.Bottom;
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

            // Start point
            StartPoint = point.ToVector2();

            // Write
            var completed = await WriteInnerAsync(page, writer, style, rect, point);

            // Store graphics state
            await page.RestoreStateAsync();

            // Adjust
            if (completed)
            {
                AdjustBottom(point, style);
            }

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
            var marginBottom = style.Margin?.Bottom ?? 0;
            point.Y += BottomAdjust + marginBottom;
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
        /// <returns>Completed or not</returns>
        protected abstract ValueTask<bool> WriteInnerAsync(IPdfPage page, PdfWriter writer, PdfStyle style, RectangleF rect, PdfPoint point);
    }
}
