using com.etsoo.EasyPdf.Content;
using com.etsoo.EasyPdf.Support;

namespace com.etsoo.EasyPdf.Fonts
{
    /// <summary>
    /// PDF font collection
    /// PDF 字体集合
    /// </summary>
    public class PdfFontCollection
    {
        private readonly List<PdfBaseFont> BaseFonts = [];

        private readonly Dictionary<string, List<IPdfFont>> Fonts = [];

        /// <summary>
        /// Load font
        /// 加载字体
        /// </summary>
        /// <param name="file">Font file</param>
        /// <returns>Result</returns>
        /// <exception cref="InvalidDataException">Font file is not a valid file</exception>
        public async Task LoadAsync(string file)
        {
            var ext = Path.GetExtension(file).ToLower();
            var ttc = ext.Equals(".ttc");

            // Only support .ttc, .ttf and .otf file
            if (!ttc && !ext.Equals(".ttf") && !ext.Equals(".otf"))
            {
                throw new InvalidDataException(nameof(file));
            }

            try
            {
                // File stream
                await using var stream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read);

                using var rf = new PdfRandomAccessFileOrArray(stream);

                if (ttc)
                {
                    // https://docs.microsoft.com/en-us/typography/opentype/spec/otff#ttc-header
                    // ttcTag
                    var mainTag = rf.ReadString(4);
                    if (!mainTag.Equals("ttcf"))
                        throw new Exception($"{file} is not a valid ttc file");

                    // Ignored majorVersion, and minorVersion fields
                    rf.SkipBytes(4);

                    // Fonts under the family
                    var numFonts = rf.ReadInt();

                    // Array of offsets to the TableDirectory for each font from the beginning of the file
                    var offsets = new int[numFonts];
                    for (var f = 0; f < numFonts; f++)
                    {
                        offsets[f] = rf.ReadInt();
                    }

                    foreach (var offset in offsets)
                    {
                        rf.Seek(offset);
                        ParseFont(rf);
                    }
                }
                else
                {
                    ParseFont(rf);
                }
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Create font
        /// 创建字体
        /// </summary>
        /// <param name="familyName">Family name</param>
        /// <param name="size">Size</param>
        /// <param name="style">Style</param>
        /// <returns>Font</returns>
        public IPdfFont CreateFont(string familyName, float size, PdfFontStyle style = PdfFontStyle.Regular)
        {
            // Match base fonts
            var baseFonts = BaseFonts.Where(
                bf => bf.Names.Any(
                    n => (n.NameId == PdfBaseFont.FontNameId.PreferredFamilyName || n.NameId == PdfBaseFont.FontNameId.FamilyName)
                        && n.Name.Equals(familyName, StringComparison.OrdinalIgnoreCase)
                )
            );
            if (!baseFonts.Any())
            {
                throw new Exception($"{familyName} does not exist");
            }

            // With the command sequence 0.4 w 2 Tr which will cause Normal text to become more "bold"
            // As for italic, most fonts contain a metric indicating their italic angle,
            // and you can use this to add a faux italic using a shear CTM transformation matrix with the cm operation.

            // Match style
            var styleFont = baseFonts.FirstOrDefault(
                bf => bf.Names.Any(
                    n => (n.NameId == PdfBaseFont.FontNameId.PreferredSubfamily || n.NameId == PdfBaseFont.FontNameId.SubfamilyName)
                        && n.Name.Equals(style.ToString(), StringComparison.OrdinalIgnoreCase)
                )
            );

            bool isMatch;
            if (styleFont == null)
            {
                isMatch = false;
                styleFont = baseFonts.First();
            }
            else
            {
                isMatch = true;
            }

            // Same font and style existing?
            var key = styleFont.UniqueIdentifier;
            if (Fonts.TryGetValue(key, out var fonts))
            {
                // Same size and style existing
                var fontExsiting = fonts.FirstOrDefault(f => f.Size == size && f.Style == style);
                if (fontExsiting != null) return fontExsiting;

                var firstFont = fonts.First();

                // Same base font
                var fontOne = new PdfFont(styleFont, isMatch, size, style, firstFont.RefName)
                {
                    ObjRef = firstFont.ObjRef
                };

                fonts.Add(fontOne);
                return fontOne;
            }

            var font = new PdfFont(styleFont, isMatch, size, style, "F" + Fonts.Count);
            Fonts.Add(key, [font]);

            return font;
        }

        private void ParseFont(PdfRandomAccessFileOrArray rf)
        {
            // sfntVersion
            // 0x00010000 or 0x4F54544F ('OTTO')
            var ttId = rf.ReadInt();
            if (ttId != 0x00010000 && ttId != 0x4F54544F)
                throw new Exception($"The file is not a valid ttf or otf file");

            var font = PdfBaseFont.Parse(rf);
            BaseFonts.Add(font);
        }

        /// <summary>
        /// Write fonts to document
        /// 输出字体
        /// </summary>
        /// <param name="writer">Writer</param>
        /// <returns>Task</returns>
        public async Task WriteAsyc(PdfWriter writer)
        {
            foreach (var item in Fonts)
            {
                // Just care the font and style, not the size
                var font = item.Value.First();

                // Write the font
                await font.WriteFontAsync(writer);
            }
        }
    }
}