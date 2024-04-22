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

            // Document
            var doc = writer.Document;

            // Document rectangle, based on page size
            var (docRect, _) = doc.Style.GetRectangle(Data.PageSize);

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

            if (border.SameStyle)
            {
                // Reduce operators for simple style
                var width = border.Left.Width;
                var widthHalf = width / 2.0F;
                rect.Inflate(-widthHalf, -widthHalf);

                await Stream.WriteAsync(PdfOperator.RG(border.Left.Color));
                await Stream.WriteAsync(PdfOperator.Zw(width));
                await Stream.WriteAsync(PdfOperator.Zre(rect));
                await Stream.WriteAsync(PdfOperator.S);
            }
            else
            {
                var leftWidth = border.Left.Width;
                var leftWidthHalf = leftWidth / 2.0F;

                var topWidth = border.Top.Width;
                var topWidthHalf = topWidth / 2.0F;

                var rightWidth = border.Right.Width;
                var rightWidthHalf = rightWidth / 2.0F;

                var bottomWidth = border.Bottom.Width;
                var bottomWidthHalf = bottomWidth / 2.0F;

                // Extended half width
                float fx, fy, tx, ty;

                if (leftWidth > 0)
                {
                    fx = rect.X - leftWidthHalf;
                    fy = rect.Y - bottomWidth;

                    tx = fx;
                    ty = rect.Y + rect.Height + topWidth;

                    await Stream.WriteAsync(PdfOperator.Zm(new Vector2(fx, fy)));
                    await Stream.WriteAsync(PdfOperator.RG(border.Left.Color));
                    await Stream.WriteAsync(PdfOperator.Zw(leftWidth));
                    await Stream.WriteAsync(PdfOperator.Zl(new Vector2(tx, ty)));
                    await Stream.WriteAsync(PdfOperator.S);
                }

                if (topWidth > 0)
                {
                    fx = rect.X - leftWidth;
                    fy = rect.Y + rect.Height + topWidthHalf;

                    tx = rect.X + rect.Width + rightWidth;
                    ty = fy;

                    await Stream.WriteAsync(PdfOperator.Zm(new Vector2(fx, fy)));
                    await Stream.WriteAsync(PdfOperator.RG(border.Top.Color));
                    await Stream.WriteAsync(PdfOperator.Zw(topWidth));
                    await Stream.WriteAsync(PdfOperator.Zl(new Vector2(tx, ty)));
                    await Stream.WriteAsync(PdfOperator.S);
                }

                if (rightWidth > 0)
                {
                    fx = rect.X + rect.Width + rightWidthHalf;
                    fy = rect.Y + rect.Height + topWidth;

                    tx = fx;
                    ty = rect.Y - bottomWidth;

                    await Stream.WriteAsync(PdfOperator.Zm(new Vector2(fx, fy)));
                    await Stream.WriteAsync(PdfOperator.RG(border.Right.Color));
                    await Stream.WriteAsync(PdfOperator.Zw(rightWidth));
                    await Stream.WriteAsync(PdfOperator.Zl(new Vector2(tx, ty)));
                    await Stream.WriteAsync(PdfOperator.S);
                }

                if (bottomWidth > 0)
                {
                    fx = rect.X + rect.Width + rightWidth;
                    fy = rect.Y - bottomWidthHalf;

                    tx = rect.X - leftWidth;
                    ty = fy;

                    await Stream.WriteAsync(PdfOperator.Zm(new Vector2(fx, fy)));
                    await Stream.WriteAsync(PdfOperator.RG(border.Bottom.Color));
                    await Stream.WriteAsync(PdfOperator.Zw(bottomWidth));
                    await Stream.WriteAsync(PdfOperator.Zl(new Vector2(tx, ty)));
                    await Stream.WriteAsync(PdfOperator.S);
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
