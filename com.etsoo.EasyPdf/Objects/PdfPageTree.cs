using com.etsoo.EasyPdf.Types;

namespace com.etsoo.EasyPdf.Objects
{
    /// <summary>
    /// PDF page tree object
    /// PDF 页面树对象
    /// </summary>
    internal class PdfPageTree : PdfObjectDic
    {
        /// <summary>
        /// Mediabox field name
        /// </summary>
        const string MediaBoxField = "MediaBox";

        /// <summary>
        /// Add media box
        /// </summary>
        /// <param name="data">Page data</param>
        /// <param name="dic">Dictionary</param>
        public static void AddMediaBox(PdfPageData data, PdfDictionary dic)
        {
            dic.AddNameRect(MediaBoxField, data.MediaBox);
        }

        public override string Type => "Pages";

        public override PdfObject Obj { get => base.Obj!; }

        /// <summary>
        /// The immediate parent
        /// </summary>
        public PdfObject? Parent { get; set; }

        /// <summary>
        /// An array of indirect references to the immediate children of this node
        /// </summary>
        public List<PdfObject> Kids { get; }

        /// <summary>
        /// The number of leaf nodes (page objects)
        /// </summary>
        public int Count => Kids.Count;

        /// <summary>
        /// Shared page data
        /// 共享的页数据
        /// </summary>
        public PdfPageData PageData { get; }

        /// <summary>
        /// Constructor
        /// 构造函数
        /// </summary>
        /// <param name="obj">Obj</param>
        /// <param name="pageData">Shared page data</param>
        public PdfPageTree(PdfObject obj, PdfPageData pageData) : base(obj)
        {
            Obj = obj;
            PageData = pageData;
            Kids = [];
        }

        protected override void AddItems()
        {
            base.AddItems();

            Dic.AddNameItem(nameof(Parent), Parent);
            Dic.AddNameArray(nameof(Kids), Kids.ToArray());
            Dic.AddNameInt(nameof(Count), Count);

            AddMediaBox(PageData, Dic);
        }
    }
}
