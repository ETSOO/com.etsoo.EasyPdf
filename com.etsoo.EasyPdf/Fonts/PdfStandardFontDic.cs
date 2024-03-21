using com.etsoo.EasyPdf.Objects;

namespace com.etsoo.EasyPdf.Fonts
{
    internal enum PdfStandardFontEncoding
    {
        StandardEncoding,
        MacRomanEncoding,
        WinAnsiEncoding,
        PDFDocEncoding
    }

    internal class PdfStandardFontDic : PdfObjectDic
    {
        public override string Type => "Font";

        public string Subtype => "Type1";

        public string BaseFont { get; }

        public PdfStandardFontEncoding Encoding { get; }

        public PdfStandardFontDic(string baseFont, PdfStandardFontEncoding encoding = PdfStandardFontEncoding.WinAnsiEncoding)
            : base()
        {
            BaseFont = baseFont;
            Encoding = encoding;
        }

        protected override void AddItems()
        {
            base.AddItems();

            Dic.AddNames(nameof(Subtype), Subtype);
            Dic.AddNames(nameof(BaseFont), BaseFont);
            Dic.AddNames(nameof(Encoding), Encoding.ToString());
        }
    }
}
