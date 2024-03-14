using com.etsoo.EasyPdf.Types;

namespace com.etsoo.EasyPdf.Objects
{
    /// <summary>
    /// PDF catalog
    /// PDF 目录
    /// </summary>
    public class PdfCatalog : PdfObjectDic
    {
        public override string Type => "Catalog";

        public string? Lang { get; set; }

        public string? Version { get; set; }

        public PdfObject Pages { get; set; }

        public PdfCatalog(PdfObject pages) : base()
        {
            Pages = pages;
        }

        public PdfCatalog(PdfObject obj, PdfDictionary dic) : base(obj, dic)
        {
            Lang = dic.GetValue<string>(nameof(Lang));
            Version = dic.GetValue<string>(nameof(Version));
            Pages = dic.GetRequired<PdfObject>(nameof(Pages));
        }

        protected override void AddItems()
        {
            base.AddItems();

            Dic.AddNameStr(nameof(Lang), Lang);
            Dic.AddNameStr(nameof(Version), Version);
            Dic.AddNameItem(nameof(Pages), Pages);
        }
    }
}
