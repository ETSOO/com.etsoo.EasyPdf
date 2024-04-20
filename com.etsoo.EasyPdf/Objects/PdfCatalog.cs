using com.etsoo.EasyPdf.Types;

namespace com.etsoo.EasyPdf.Objects
{
    /// <summary>
    /// PDF catalog
    /// PDF 目录
    /// </summary>
    internal class PdfCatalog : PdfObjectDic
    {
        public override string Type => "Catalog";

        public string? Lang { get; set; }

        public string? Version { get; set; }

        public PdfObject Pages { get; set; }

        public PdfLinkBaseDic? URI { get; set; }

        public PdfCatalog(PdfObject pages) : base()
        {
            Pages = pages;
        }

        protected override async Task AddItemsAsync()
        {
            await base.AddItemsAsync();

            Dic.AddNameStr(nameof(Lang), Lang);
            Dic.AddNameStr(nameof(Version), Version);
            Dic.AddNameItem(nameof(Pages), Pages);
            Dic.AddNameItem(nameof(URI), URI);
        }
    }
}
