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
        public IPdfType? Dest { get; set; }

        public string? H { get; set; }

        public PdfLinkAnnotation(Rectangle rect) : base("Link", rect)
        {
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
