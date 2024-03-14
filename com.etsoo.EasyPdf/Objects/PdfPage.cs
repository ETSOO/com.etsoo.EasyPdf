using com.etsoo.EasyPdf.Content;
using com.etsoo.EasyPdf.Dto;
using com.etsoo.EasyPdf.Types;
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

        /// <summary>
        /// Last identifier point
        /// 最后标识点
        /// </summary>
        private Vector2 lastPoint = new();

        private int marginLeft;
        private int marginTop;

        /// <summary>
        /// Constructor
        /// 构造函数
        /// </summary>
        /// <param name="obj">Obj</param>
        /// <param name="parent">Parent page tree</param>
        /// <param name="pageData">Page data</param>
        /// <param name="style">Parent style</param>
        public PdfPage(PdfObject obj, PdfPageTree parent, PdfPageData pageData, PdfStyle style) : base(obj)
        {
            Parent = parent;
            ParentObj = parent.Obj.AsRef();
            Stream = PdfConstants.StreamManager.GetStream();
            Index = parent.Count;
            Data = pageData;
            Style = style;
        }

        /*
        public PdfPage(PdfObject obj, PdfDictionary dic) : base(obj, dic)
        {
            Parent = dic.GetRequired<PdfObject>("Parent");

            LastModified = dic.GetValue<DateTime?>("LastModified");

            var mediaBoxArray = dic.Get<PdfArray>("MediaBox");
            PageSize = mediaBoxArray.ToRectangle();

            Contents = dic.Get<PdfObject>("Contents");

            // Match the stream with contents
            Stream = default!;

            Rotate = dic.GetValue<int?>("Rotate");

            // Resources
            var resources = dic.GetRequired<PdfDictionary>("Resources");
        }
        */

        protected override void AddItems()
        {
            base.AddItems();

            Dic.AddNameItem(nameof(Parent), ParentObj);

            if (!Data.PageSize.Equals(Parent.PageData.PageSize))
            {
                // No necessary to write duplicate size
                PdfPageTree.AddMediaBox(Data, Dic);
            }

            Dic.AddNameInt(nameof(Data.Rotate), Data.Rotate);
            Dic.AddNameItem(nameof(Contents), Contents);
            Dic.AddNameItem(nameof(Resources), Resources);
            Dic.AddNameDate(nameof(LastModified), LastModified);
        }

        public Vector2 CalculatePoint(Vector2 point)
        {
            return point with
            {
                X = point.X + marginLeft,
                Y = marginTop - point.Y
            };
        }

        public async Task PrepareAsync()
        {
            // Page data should be frozon now
            marginLeft = Style.Margin?.Left ?? 0;
            marginTop = Data.PageSize.Height - Style.Margin?.Top ?? 0;

            await Stream.WriteAsync(PdfOperator.Zm(CalculatePoint(lastPoint)));
        }

        private bool gStateSaved;

        // Restore graphics state
        private void RestoreGState()
        {
            if (gStateSaved)
            {
                Stream.Write(PdfOperator.Q);

                gStateSaved = false;
            }
        }

        // Save graphics state
        private void SaveGState()
        {
            Stream.Write(PdfOperator.q);
            gStateSaved = true;
        }

        public async Task WriteEndAsync()
        {
            // await Stream.WriteAsync(PdfOperator.ET);

            // Back to begin
            Stream.Position = 0;
        }

        public async Task<bool> WriteAsync(PdfBlock block, IPdfWriter writer)
        {
            await block.WriteAsync(this, writer);

            return false;
        }
    }
}
