using com.etsoo.EasyPdf.Types;
using System.Drawing;

namespace com.etsoo.EasyPdf.Objects
{
    /// <summary>
    /// Link annotation
    /// 12.5.6.5, Table 173 – Additional entries specific to a link annotation
    /// 链接注释
    /// </summary>
    internal class PdfLinkAnnotation : PdfAnnotation
    {
        /// <summary>
        /// Can also be PdfAction
        /// </summary>
        public IPdfType A { get; set; }

        public IPdfType? Dest { get; set; }

        public string? H { get; set; }

        public PdfLinkAnnotation(IPdfType a, RectangleF rect) : base("Link", rect)
        {
            A = a;
            Border = new PdfArray(0, 0, 0);
        }

        protected override async Task AddItemsAsync()
        {
            await base.AddItemsAsync();

            Dic.AddNameItem(nameof(A), A);
            Dic.AddNameItem(nameof(Dest), Dest);
            Dic.AddNames(nameof(H), H);
        }
    }

    internal record PdfLinkBaseDic : PdfDictionary
    {
        public PdfLinkBaseDic(string? baseUri) : base()
        {
            if (!string.IsNullOrEmpty(baseUri))
            {
                AddNameStr("Base", baseUri);
            }
        }
    }
}
