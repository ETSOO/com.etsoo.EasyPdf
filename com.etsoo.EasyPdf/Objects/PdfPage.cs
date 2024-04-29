using com.etsoo.EasyPdf.Content;
using com.etsoo.EasyPdf.Support;
using com.etsoo.EasyPdf.Types;
using System.Drawing;
using System.Numerics;

namespace com.etsoo.EasyPdf.Objects
{
    /// <summary>
    /// PDF page object
    /// PDF 页面对象
    /// </summary>
    internal class PdfPage : PdfObjectDic, IPdfPage
    {
        /// <summary>
        /// Type
        /// 类型
        /// </summary>
        public override string Type => "Page";

        /// <summary>
        /// Obj reference
        /// 对象引用
        /// </summary>
        public override PdfObject Obj { get => base.Obj!; }

        /// <summary>
        /// Parent tree
        /// 父树
        /// </summary>
        public PdfPageTree Parent { get; }

        /// <summary>
        /// Parent tree object reference
        /// 父树对象引用
        /// </summary>
        public PdfObject ParentObj { get; }

        /// <summary>
        /// Resources
        /// 资源
        /// </summary>
        public PdfPageResource Resources { get; } = new PdfPageResource();

        /// <summary>
        /// Last modified time
        /// 上次修改时间
        /// </summary>
        public DateTime? LastModified { get; set; }

        /// <summary>
        /// Annotation dictionary array
        /// 注释字典数组
        /// </summary>
        public List<PdfObject> Annots { get; } = [];

        /// <summary>
        /// Content stream
        /// 内容流
        /// </summary>
        public PdfObject? Contents { get; set; }

        /// <summary>
        /// Stream to write
        /// 写入流
        /// </summary>
        public Stream Stream { get; }

        /// <summary>
        /// Page data
        /// 页面数据
        /// </summary>
        public PdfPageData Data { get; }

        /// <summary>
        /// Page index
        /// 页面索引
        /// </summary>
        public int Index { get; }

        /// <summary>
        /// Style
        /// 样式
        /// </summary>
        public PdfStyle Style { get; }

        private RectangleF pageFullRect;

        /// <summary>
        /// Page's full rectangle
        /// 页面的完整矩形
        /// </summary>
        public RectangleF PageFullRect => pageFullRect;

        private RectangleF contentRect;

        /// <summary>
        /// Page's content rectangle
        /// 页面的内容矩形
        /// </summary>
        public RectangleF ContentRect => contentRect;

        private readonly PdfPoint currentPoint = new();

        /// <summary>
        /// Current drawing point inside the content rectangle
        /// 内容矩形内的当前绘制点
        /// </summary>
        public PdfPoint CurrentPoint => currentPoint;

        /// <summary>
        /// Constructor
        /// 构造函数
        /// </summary>
        /// <param name="obj">Obj</param>
        /// <param name="parent">Parent page tree</param>
        /// <param name="pageData">Page data</param>
        /// <param name="style">Parent style</param>
        internal PdfPage(PdfObject obj, PdfPageTree parent, PdfPageData pageData, PdfStyle style) : base(obj)
        {
            Parent = parent;
            ParentObj = parent.Obj.AsRef();
            Stream = PdfConstants.StreamManager.GetStream();
            Index = parent.Count;
            Data = pageData;
            Style = style;
        }

        protected override async Task AddItemsAsync()
        {
            await base.AddItemsAsync();

            Dic.AddNameItem(nameof(Parent), ParentObj);

            if (!Data.PageSize.Equals(Parent.PageData.PageSize))
            {
                // No necessary to write duplicate size
                PdfPageTree.AddMediaBox(Data, Dic);
            }

            Dic.AddNameInt(nameof(Data.Rotate), Data.Rotate);
            // Dic.AddNameNum("UserUnit", 1.0);
            Dic.AddNameArray(nameof(Annots), Annots);
            Dic.AddNameItem(nameof(Contents), Contents);
            Dic.AddNameItem(nameof(Resources), Resources);
            Dic.AddNameDate(nameof(LastModified), LastModified);
        }

        /// <summary>
        /// Begin text output
        /// 开始文本输出
        /// </summary>
        /// <returns>Task</returns>
        public async Task BeginTextAsync()
        {
            await Stream.WriteAsync(PdfOperator.BT);
        }

        /// <summary>
        /// Calculate point
        /// 计算点
        /// </summary>
        /// <param name="point">Point</param>
        /// <returns>Result</returns>
        public Vector2 CalculatePoint(Vector2 point)
        {
            return point with
            {
                X = point.X + contentRect.X,
                Y = contentRect.Height + contentRect.Y - point.Y
            };
        }

        /// <summary>
        /// Calculate current point
        /// 计算当前点
        /// </summary>
        /// <returns>Result</returns>
        public Vector2 CalculatePoint()
        {
            return CalculatePoint(CurrentPoint.ToVector2());
        }

        /// <summary>
        /// End text output
        /// 结束文本输出
        /// </summary>
        /// <returns>Task</returns>
        public async Task EndTextAsync()
        {
            await Stream.WriteAsync(PdfOperator.ET);
        }

        private RectangleF GetPageRectangle(RectangleF docRect, PdfStyle style)
        {
            // box-sizing: border-box behavior
            var adjustLeft = (style.Padding?.Left ?? 0) + (style.Border?.Left?.Width ?? 0);
            var adjustTop = (style.Padding?.Top ?? 0) + (style.Border?.Top?.Width ?? 0);
            var adjustRight = (style.Padding?.Right ?? 0) + (style.Border?.Right?.Width ?? 0);
            var adjustBottom = (style.Padding?.Bottom ?? 0) + (style.Border?.Bottom?.Width ?? 0);

            var x = docRect.X + adjustLeft;
            var y = docRect.Y + adjustBottom; // PDF coordinate is bottom to top
            var width = docRect.Width - adjustLeft - adjustRight;
            var height = docRect.Height - adjustTop - adjustBottom;

            return new RectangleF(x, y, width, height);
        }

        /// <summary>
        /// Move to the current point operator
        /// 移动到当前点操作
        /// </summary>
        /// <param name="adjust">The adjustment</param>
        /// <returns>Bytes</returns>
        public byte[] CurrentPointOperator(Vector2? adjust = null)
        {
            var point = currentPoint.ToVector2();
            if (adjust.HasValue)
            {
                point.X += adjust.Value.X;
                point.Y += adjust.Value.Y;
            }

            return MovingOperator(point, false);
        }

        /// <summary>
        /// Operator for Moving to the point
        /// </summary>
        /// <param name="globalPoint">Global point</param>
        /// <param name="cm">cm vs Tm</param>
        /// <returns>Bytes</returns>
        protected byte[] MovingOperator(Vector2 globalPoint, bool cm = true)
        {
            return cm ? PdfOperator.Tm(globalPoint, cm) : PdfOperator.Td(globalPoint);
        }

        /// <summary>
        /// Move to the point
        /// 移动到点
        /// </summary>
        /// <param name="point">The local point</param>
        /// <returns>Global drawing point</returns>
        public async Task<Vector2> MoveToAsync(Vector2 point)
        {
            UpdatePoint(point);
            var globalPoint = CalculatePoint(point);
            await Stream.WriteAsync(PdfOperator.Zm(globalPoint));
            return globalPoint;
        }

        /// <summary>
        /// Move text output to the point (cm)
        /// 移动文本输出到点
        /// </summary>
        /// <param name="point">Start point</param>
        /// <param name="lineHeight">Line height</param>
        /// <returns>Global drawing point</returns>
        public async Task<Vector2> MoveToAsync(Vector2 point, float lineHeight)
        {
            point.Y += lineHeight;
            UpdatePoint(point);

            var globalPoint = CalculatePoint(point);

            // Deduct line height
            // Move to the next line
            await Stream.WriteAsync(MovingOperator(globalPoint));

            return globalPoint;
        }

        public async Task PrepareAsync(PdfWriter writer)
        {
            // Page style
            var style = Style.GetComputedStyle();

            // Page rectangle
            pageFullRect = new RectangleF(PointF.Empty, Data.PageSize);

            // Document
            var doc = writer.Document;

            // Document rectangle, based on page size
            var (docRect, _) = doc.Style.GetRectangle(pageFullRect);

            // Document border & background
            await WriteBorderAndBackgroundAsync(doc.Style, docRect);

            // Page Border & background
            var pageRect = GetPageRectangle(docRect, doc.Style);
            await WriteBorderAndBackgroundAsync(style, pageRect);

            // Page available content size
            contentRect = GetPageRectangle(pageRect, style);
        }

        /// <summary>
        /// Restore graphics state
        /// 恢复图形状态
        /// </summary>
        /// <returns>Task</returns>
        public async Task RestoreStateAsync()
        {
            await Stream.WriteAsync(PdfOperator.Q);
        }

        /// <summary>
        /// Save graphics state
        /// 保持图形状态
        /// </summary>
        /// <returns></returns>
        public async Task SaveStateAsync()
        {
            await Stream.WriteAsync(PdfOperator.q);
        }

        /// <summary>
        /// Set font color
        /// 设置字体颜色
        /// </summary>
        /// <param name="color">Color</param>
        /// <returns>Task</returns>
        public async Task SetFontColor(PdfColor? color)
        {
            if (color.HasValue)
            {
                await Stream.WriteAsync(PdfOperator.RG2(color.Value));
            }
        }

        /// <summary>
        /// Update drawing point
        /// 更新绘制点
        /// </summary>
        public void UpdatePoint(Vector2 point)
        {
            currentPoint.X = point.X;
            currentPoint.Y = point.Y;
        }

        /// <summary>
        /// Write block elment
        /// 输出块元素
        /// </summary>
        /// <param name="block">Block element</param>
        /// <param name="writer">Writer</param>
        /// <returns>Task</returns>
        public async Task WriteAsync(PdfBlock block, PdfWriter writer)
        {
            var fontSize = Style.FontSize ?? Style.Parent?.FontSize ?? 12;
            block.SetParentStyle(Style, fontSize);
            await block.WriteAsync(this, writer);
        }

        /// <summary>
        /// Write background
        /// 输出背景
        /// </summary>
        /// <param name="bgcolor">Background color</param>
        /// <param name="rect">Rectangle</param>
        /// <returns>Task</returns>
        public async Task WriteBackgroundAsync(PdfColor bgcolor, RectangleF rect)
        {
            // Save graphics state
            await SaveStateAsync();

            await Stream.WriteAsync(PdfOperator.RG2(bgcolor));
            await Stream.WriteAsync(PdfOperator.Zw(0));
            await Stream.WriteAsync(PdfOperator.Zre(rect));
            await Stream.WriteAsync(PdfOperator.B);

            // Restore graphics state
            await RestoreStateAsync();
        }

        private async Task CreateDashedLineXAsync(float width, Vector2 start, Vector2 end)
        {
            if (width < 1.5) width = 1.5f;

            var (w, g) = CalculateDashedGap(end.X - start.X, width);

            var step = 0;
            while (start.X < end.X)
            {
                if (step % 2 == 0)
                {
                    start.X += w;

                    if (start.X > end.X)
                        await Stream.WriteAsync(PdfOperator.Zl(end));
                    else
                        await Stream.WriteAsync(PdfOperator.Zl(start));
                }
                else
                {
                    start.X += g;
                    await Stream.WriteAsync(PdfOperator.Zm(start));
                }

                step++;
            }
        }

        private (float width, float gap) CalculateDashedGap(float width, float gap, int multiple = 2)
        {
            var count = Convert.ToInt32((width + gap) / ((1 + multiple) * gap));
            var newGap = (width - multiple * gap * count) / (count - 1);
            return (multiple * gap, newGap);
        }

        private async Task CreateDashedLineYAsync(float width, Vector2 start, Vector2 end)
        {
            if (width < 1.5) width = 1.5f;

            var (w, g) = CalculateDashedGap(end.Y - start.Y, width);

            var step = 0;
            while (start.Y < end.Y)
            {
                if (step % 2 == 0)
                {
                    start.Y += w;

                    if (start.Y > end.Y)
                        await Stream.WriteAsync(PdfOperator.Zl(end));
                    else
                        await Stream.WriteAsync(PdfOperator.Zl(start));
                }
                else
                {
                    start.Y += g;
                    await Stream.WriteAsync(PdfOperator.Zm(start));
                }

                step++;
            }
        }

        private async Task CreateDottedLineXAsync(float width, Vector2 start, Vector2 end)
        {
            if (width < 1.5) width = 1.5f;

            var (w, g) = CalculateDashedGap(end.X - start.X, width, 1);

            var step = 0;
            while (start.X < end.X)
            {
                if (step % 2 == 0)
                {
                    await Stream.WriteAsync(PdfOperator.Circle(start.X + w / 2, start.Y, w / 2));
                    start.X += w;
                }
                else
                {
                    start.X += g;
                    await Stream.WriteAsync(PdfOperator.Zm(start));
                }

                step++;
            }
        }

        private async Task CreateDottedLineYAsync(float width, Vector2 start, Vector2 end)
        {
            if (width < 1.5) width = 1.5f;

            var (w, g) = CalculateDashedGap(end.Y - start.Y, width, 1);

            var step = 0;
            while (start.Y < end.Y)
            {
                if (step % 2 == 0)
                {
                    await Stream.WriteAsync(PdfOperator.Circle(start.X, start.Y + w / 2, w / 2));
                    start.Y += w;
                }
                else
                {
                    start.Y += g;
                    await Stream.WriteAsync(PdfOperator.Zm(start));
                }

                step++;
            }
        }

        private async Task DrawBorderAsync(float width, PdfColor color, PdfStyleBorderStyle style, Vector2 start, Vector2 end, bool vertical)
        {
            if (style == PdfStyleBorderStyle.Dotted)
            {
                await Stream.WriteAsync(PdfOperator.Zw(0));
                await Stream.WriteAsync(PdfOperator.RG2(color));

                if (vertical)
                    await CreateDottedLineYAsync(width, start, end);
                else
                    await CreateDottedLineXAsync(width, start, end);

                await Stream.WriteAsync(PdfOperator.B);
            }
            else
            {
                await Stream.WriteAsync(PdfOperator.RG(color));
                await Stream.WriteAsync(PdfOperator.Zw(width));
                await Stream.WriteAsync(PdfOperator.Zm(start));

                if (style == PdfStyleBorderStyle.Dashed)
                {
                    if (vertical)
                        await CreateDashedLineYAsync(width, start, end);
                    else
                        await CreateDashedLineXAsync(width, start, end);
                }
                else
                {
                    await Stream.WriteAsync(PdfOperator.Zl(end));
                }

                await Stream.WriteAsync(PdfOperator.S);
            }
        }

        private async Task WriteLeftBorderAsync(RectangleF rect, float width, PdfColor color, PdfStyleBorderStyle style)
        {
            var fx = rect.X + width / 2;
            var fy = rect.Y;
            var start = new Vector2(fx, fy);

            var tx = fx;
            var ty = rect.Y + rect.Height;
            var end = new Vector2(tx, ty);

            await DrawBorderAsync(width, color, style, start, end, true);
        }

        private async Task WriteTopBorderAsync(RectangleF rect, float width, PdfColor color, PdfStyleBorderStyle style)
        {
            var fx = rect.X;
            var fy = rect.Y + rect.Height - width / 2;
            var start = new Vector2(fx, fy);

            var tx = rect.X + rect.Width;
            var ty = fy;
            var end = new Vector2(tx, ty);

            await DrawBorderAsync(width, color, style, start, end, false);
        }

        private async Task WriteRightBorderAsync(RectangleF rect, float width, PdfColor color, PdfStyleBorderStyle style)
        {
            var fx = rect.X + rect.Width - width / 2;
            var fy = rect.Y + rect.Height;
            var end = new Vector2(fx, fy);

            var tx = fx;
            var ty = rect.Y;
            var start = new Vector2(tx, ty);

            await DrawBorderAsync(width, color, style, start, end, true);
        }

        private async Task WriteBottomBorderAsync(RectangleF rect, float width, PdfColor color, PdfStyleBorderStyle style)
        {
            var fx = rect.X + rect.Width;
            var fy = rect.Y + width / 2;
            var end = new Vector2(fx, fy);

            var tx = rect.X;
            var ty = fy;
            var start = new Vector2(tx, ty);

            await DrawBorderAsync(width, color, style, start, end, false);
        }

        /// <summary>
        /// Write border
        /// 输出边框
        /// </summary>
        /// <param name="border">Border style</param>
        /// <param name="rect">Rectangle</param>
        /// <returns>Task</returns>
        public async Task WriteBorderAsync(PdfStyleBorder border, RectangleF rect)
        {
            // Save graphics state
            await SaveStateAsync();

            var left = border.Left;
            if (border.SameStyle && (left.Style == PdfStyleBorderStyle.Solid))
            {
                // Reduce operators for simple style
                var width = left.Width;
                var widthHalf = width / 2.0F;
                rect.Inflate(-widthHalf, -widthHalf);

                await Stream.WriteAsync(PdfOperator.RG(left.Color));
                await Stream.WriteAsync(PdfOperator.Zw(width));
                await Stream.WriteAsync(PdfOperator.Zre(rect));
                await Stream.WriteAsync(PdfOperator.S);
            }
            else
            {
                var leftWidth = left.Width;
                var topWidth = border.Top.Width;
                var rightWidth = border.Right.Width;
                var bottomWidth = border.Bottom.Width;

                if (leftWidth > 0 && border.Left.Style != PdfStyleBorderStyle.None)
                {
                    await WriteLeftBorderAsync(rect, leftWidth, border.Left.Color, border.Left.Style);
                }

                if (topWidth > 0 && border.Top.Style != PdfStyleBorderStyle.None)
                {
                    await WriteTopBorderAsync(rect, topWidth, border.Top.Color, border.Top.Style);
                }

                if (rightWidth > 0 && border.Right.Style != PdfStyleBorderStyle.None)
                {
                    await WriteRightBorderAsync(rect, rightWidth, border.Right.Color, border.Right.Style);
                }

                if (bottomWidth > 0 && border.Bottom.Style != PdfStyleBorderStyle.None)
                {
                    await WriteBottomBorderAsync(rect, bottomWidth, border.Bottom.Color, border.Bottom.Style);
                }
            }

            // Restore graphics state
            await RestoreStateAsync();
        }

        /// <summary>
        /// Write border and background
        /// 输出边框和背景
        /// </summary>
        /// <param name="style">Current style</param>
        /// <param name="rect">Rectangle</param>
        /// <returns>Task</returns>
        public async ValueTask WriteBorderAndBackgroundAsync(PdfStyle style, RectangleF rect)
        {
            var bgcolor = style.BackgroundColor;
            if (bgcolor.HasValue)
            {
                await WriteBackgroundAsync(bgcolor.Value, rect);
            }

            var border = style.Border;
            if (border?.HasBorder is true)
            {
                await WriteBorderAsync(border, rect);
            }
        }

        /// <summary>
        /// Page writing end
        /// 页面写入结束
        /// </summary>
        /// <param name="writer">Writer</param>
        /// <returns>Task</returns>
        public async Task WriteEndAsync(PdfWriter writer)
        {
            // Finish writing
            await using (Stream)
            {
                // Back to the beginning
                Stream.Position = 0;

                // Pdf content stream
                var pageStream = new PdfStreamDic(Stream);

                // Set content
                Contents = await writer.WriteDicAsync(pageStream);

                // Dispose
                await Stream.DisposeAsync();
            }
        }
    }
}
