using com.etsoo.EasyPdf.Objects;
using com.etsoo.EasyPdf.Types;

namespace com.etsoo.EasyPdf.Fonts
{
    /// <summary>
    /// PDF font dictionary
    /// The glyph coordinate system (glyph outline and its metrics) is the space in which an individual character’s glyph is defined
    /// the units of glyph space are one-thousandth of a unit of text space.
    /// The glyph bounding box shall be the smallest rectangle (oriented with the axes of the glyph coordinate system) that just encloses the entire glyph shape.
    /// The bounding box shall be expressed in terms of its left, bottom, right, and top coordinates relative to the glyph origin in the glyph coordinate system.
    /// Type 3 font, all metrics are specified explicitly, not supported
    /// PDF 字体字典
    /// </summary>
    internal class PdfFontDic : PdfObjectDic
    {
        public override string Type => "Font";

        public string Subtype => "Type0";

        public string BaseFont { get; set; }

        public string Encoding { get; set; }

        public PdfObject[]? DescendantFonts { get; set; }

        /// <summary>
        /// 9.10.3 ToUnicode CMaps
        /// </summary>
        public PdfObject? ToUnicode { get; set; }

        public PdfFontDic(string baseFont, string encoding)
        {
            BaseFont=baseFont;
            Encoding=encoding;
        }

        protected override void AddItems()
        {
            base.AddItems();

            Dic.AddNames(nameof(Subtype), Subtype);
            Dic.AddNames(nameof(BaseFont), BaseFont);
            Dic.AddNames(nameof(Encoding), Encoding);
            Dic.AddNameArray(nameof(DescendantFonts), DescendantFonts);
            Dic.AddNameItem(nameof(ToUnicode), ToUnicode);
        }
    }
}
