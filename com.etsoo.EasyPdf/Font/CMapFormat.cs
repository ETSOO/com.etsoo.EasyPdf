using System.Collections.Generic;

namespace com.etsoo.EasyPdf.Font
{
    /// <summary>
    /// CMap format data
    /// https://learn.microsoft.com/en-us/typography/opentype/spec/cmap
    /// </summary>
    internal class CMapFormat
    {
        internal FontNamePlatform Platform { get; set; }
        internal int EncodingID { get; set; }
        internal int? LanguageID { get; set; }
        internal int Format { get; set; }
        internal Dictionary<int, int> Glyphs { get; set; } = new Dictionary<int, int>();
    }
}
