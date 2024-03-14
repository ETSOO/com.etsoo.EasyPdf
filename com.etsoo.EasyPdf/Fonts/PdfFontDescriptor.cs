using com.etsoo.EasyPdf.Objects;
using com.etsoo.EasyPdf.Types;
using System.Drawing;

namespace com.etsoo.EasyPdf.Fonts
{
    /// <summary>
    /// 9.8.1, Table 122 – Entries common to all font descriptors
    /// </summary>
    internal class PdfFontDescriptor : PdfObjectDic
    {
        public override string Type => "FontDescriptor";

        public string FontName { get; }

        public string? FontFamily { get; set; }

        public RectangleF FontBBox { get; init; }

        public ushort? FontWeight { get; set; }

        public double ItalicAngle { get; set; } = 0;

        public int Flags { get; init; }

        public float Ascent { get; init; }

        public float Descent { get; init; }

        public float CapHeight { get; init; }

        public int StemV { get; init; }

        public PdfObject? FontFile2 { get; set; }

        public PdfObject? FontFile3 { get; set; }

        public PdfFontDescriptor(string fontName)
        {
            FontName = fontName;
        }

        protected override void AddItems()
        {
            base.AddItems();

            Dic.AddNames(nameof(FontName), FontName);
            Dic.AddNameBinary(nameof(FontFamily), FontFamily);
            Dic.AddNameRect(nameof(FontBBox), FontBBox);
            Dic.AddNameNum(nameof(ItalicAngle), ItalicAngle);
            Dic.AddNameInt(nameof(FontWeight), FontWeight);
            Dic.AddNameInt(nameof(Flags), Flags);
            Dic.AddNameNum(nameof(Ascent), Ascent);
            Dic.AddNameNum(nameof(Descent), Descent);
            Dic.AddNameNum(nameof(CapHeight), CapHeight);
            Dic.AddNameInt(nameof(StemV), StemV);
            Dic.AddNameItem(nameof(FontFile2), FontFile2);
            Dic.AddNameItem(nameof(FontFile3), FontFile3);
        }
    }
}
