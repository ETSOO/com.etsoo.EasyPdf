using com.etsoo.EasyPdf.Objects;
using com.etsoo.EasyPdf.Types;

namespace com.etsoo.EasyPdf.Fonts
{
    internal record PdfCIDSystemInfo : PdfDictionary
    {
        public string Registry { get; init; } = "Adobe";

        public string Ordering { get; init; } = "Identity";

        public byte Supplement { get; init; } = 0;

        public override Task WriteToAsync(Stream stream)
        {
            AddNameStr(nameof(Registry), Registry);
            AddNameStr(nameof(Ordering), Ordering);
            AddNameInt(nameof(Supplement), Supplement);

            return base.WriteToAsync(stream);
        }
    }

    /// <summary>
    /// 9.7.4, Table 117 – Entries in a CIDFont dictionary
    /// A CIDFont dictionary is a PDF object that contains information about a CIDFont program.
    /// Although its Type value is Font, a CIDFont is not actually a font. It does not have an Encoding entry.
    /// </summary>
    internal class PdfCIDFontDic : PdfObjectDic
    {
        public override string Type => "Font";

        public string Subtype { get; }

        public string BaseFont { get; }

        public PdfObject FontDescriptor { get; }

        /// <summary>
        /// A specification of the mapping from CIDs to glyph indices
        /// By using the CMap/encoding Identity-H, we basically say: "The character codes we write to the PDF files are already CIDs, you don't have to remap anything.
        /// A character encoding standard specifies how to translate the numeric value of the characters in a text into visible characters.
        /// For instance, WinAnsi encoding standard translates the numeric decimal value 174 into ® (registered) character whereas
        /// </summary>
        public string CIDToGIDMap { get; set; } = "Identity";

        public int DW { get; set; }

        public int[]? W { get; set; }

        public PdfCIDSystemInfo CIDSystemInfo { get; init; } = new PdfCIDSystemInfo();

        public PdfCIDFontDic(string subtype, string baseFont, PdfObject fontDescriptor)
        {
            Subtype = subtype;
            BaseFont=baseFont;
            FontDescriptor = fontDescriptor;
        }

        protected override async Task AddItemsAsync()
        {
            await base.AddItemsAsync();

            Dic.AddNames(nameof(Subtype), Subtype);
            Dic.AddNames(nameof(BaseFont), BaseFont);
            Dic.AddNameItem(nameof(FontDescriptor), FontDescriptor);
            Dic.AddNameItem(nameof(CIDSystemInfo), CIDSystemInfo);
            Dic.AddNames(nameof(CIDToGIDMap), CIDToGIDMap);
            Dic.AddNameInt(nameof(DW), DW);

            if (W != null)
                Dic.AddNameArray(nameof(W), W.Select(w => new PdfInt(w)));
        }
    }
}
