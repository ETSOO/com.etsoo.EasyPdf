using com.etsoo.PureIO;
using System.Text;

namespace com.etsoo.EasyPdf.Fonts
{
    internal partial class PdfBaseFont
    {
        /// <summary>
        /// The components of table 'head'
        /// This table gives global information about the font
        /// https://docs.microsoft.com/en-us/typography/opentype/spec/head
        /// </summary>
        internal class HeadTable
        {
            internal int flags { get; init; }
            internal int unitsPerEm { get; init; }
            internal short xMin { get; init; }
            internal short yMin { get; init; }
            internal short xMax { get; init; }
            internal short yMax { get; init; }
            internal int macStyle { get; init; }

            // 0 for short offsets (Offset16), 1 for long (Offset32)
            internal short indexToLocFormat { get; init; }

            internal bool locaShortVersion => indexToLocFormat == 0;
        }

        /// <summary>
        /// The components of table 'hhea', horizontal header
        /// This table contains information for horizontal layout
        /// https://docs.microsoft.com/en-us/typography/opentype/spec/hhea
        /// </summary>
        internal class HheaTable
        {
            /// <summary>
            /// The ascender, descender and linegap values in this table are Apple specific
            /// The sTypoAscender, sTypoDescender and sTypoLineGap fields in the OS/2 table are used on the Windows platform, 
            /// and are recommended for new text-layout implementations
            /// </summary>
            internal short ascender { get; init; }
            internal short descender { get; init; }
            internal short lineGap { get; init; }
            internal int advanceWidthMax { get; init; }
            internal short minLeftSideBearing { get; init; }
            internal short minRightSideBearing { get; init; }

            // Max(lsb + (xMax - xMin)).
            internal short xMaxExtent { get; init; }

            internal short caretSlopeRise { get; init; }
            internal short caretSlopeRun { get; init; }
            internal int numberOfHMetrics { get; init; }
        }

        /// <summary>
        /// The vertical header table (tag name: 'vhea')
        /// contains information needed for vertical layout of Chinese, Japanese, Korean (CJK) and other ideographic scripts
        /// https://docs.microsoft.com/en-us/typography/opentype/spec/vhea
        /// </summary>
        internal class VheaTable
        {
            /// <summary>
            /// 1.0 ascent
            /// </summary>
            internal short vertTypoAscender { get; init; }

            /// <summary>
            /// 1.0 ascent
            /// </summary>
            internal short vertTypoDescender { get; init; }

            /// <summary>
            /// 1.0 lineGap
            /// </summary>
            internal short vertTypoLineGap { get; init; }
            internal int advanceHeightMax { get; init; }
            internal short minTopSideBearing { get; init; }
            internal short minBottomSideBearing { get; init; }

            // max(tsb + (yMax - yMin))
            internal short yMaxExtent { get; init; }

            internal short caretSlopeRise { get; init; }
            internal short caretSlopeRun { get; init; }
            internal int numOfLongVerMetrics { get; init; }
        }

        /// <summary>
        /// The OS/2 table consists of a set of metrics and other data
        /// https://docs.microsoft.com/en-us/typography/opentype/spec/os2
        /// </summary>
        internal class MetricsTable
        {
            internal short xAvgCharWidth { get; init; }
            internal int usWeightClass { get; init; }
            internal int usWidthClass { get; init; }
            internal short fsType { get; init; }
            internal short ySubscriptXSize { get; init; }
            internal short ySubscriptYSize { get; init; }
            internal short ySubscriptXOffset { get; init; }
            internal short ySubscriptYOffset { get; init; }
            internal short ySuperscriptXSize { get; init; }
            internal short ySuperscriptYSize { get; init; }
            internal short ySuperscriptXOffset { get; init; }
            internal short ySuperscriptYOffset { get; init; }
            internal short yStrikeoutSize { get; init; }
            internal short yStrikeoutPosition { get; init; }
            internal short sFamilyClass { get; init; }
            internal byte[] panose { get; init; } = default!;
            internal byte[] achVendID { get; init; } = default!;
            internal int fsSelection { get; init; }
            internal int usFirstCharIndex { get; init; }
            internal int usLastCharIndex { get; init; }
            internal short sTypoAscender { get; init; }
            internal short sTypoDescender { get; init; }
            internal short sTypoLineGap { get; init; }
            internal int usWinAscent { get; init; }
            internal int usWinDescent { get; init; }
            internal int ulCodePageRange1 { get; init; }
            internal int ulCodePageRange2 { get; init; }
            internal short sCapHeight { get; init; }
        }

        /// <summary>
        /// post — PostScript Table
        /// contains additional information needed to use TrueType or OpenType™ fonts on PostScript printers
        /// https://docs.microsoft.com/en-us/typography/opentype/spec/post
        /// </summary>
        internal class PostTable
        {
            internal double italicAngle { get; init; }
            internal int underlinePosition { get; init; }
            internal int underlineThickness { get; init; }
            internal bool isFixedPitch { get; init; }

            // In 'maxp' table
            internal int numGlyphs { get; init; }
        }

        internal class HMetrics
        {
            /// <summary>
            /// Advance width(aw)
            /// </summary>
            internal ushort advanceWidth { get; init; }

            /// <summary>
            /// Glyph left side bearing(lsb)
            /// </summary>
            internal short lsb { get; init; }
        }

        internal class VMetrics
        {
            /// <summary>
            /// The advance height of the glyph
            /// </summary>
            internal ushort advanceHeight { get; init; }

            /// <summary>
            /// The top sidebearing of the glyph
            /// </summary>
            internal short topSideBearing { get; init; }
        }

        internal class FontName
        {
            internal FontNamePlatform Platform { get; init; }
            internal int EncodingID { get; init; }
            internal int LanguageID { get; init; }
            internal FontNameId NameId { get; init; }
            internal string Name { get; init; } = default!;
        }

        internal class CMapFormat
        {
            internal FontNamePlatform Platform { get; init; }
            internal int EncodingID { get; init; }
            internal int? LanguageID { get; init; }
            internal int Format { get; init; }
            internal Dictionary<int, int> Glyphs { get; init; } = default!;
        }

        /// <summary>
        /// Platform, then see Platform-specific encoding IDs
        /// </summary>
        internal enum FontNamePlatform : byte
        {
            Unicode = 0,

            Macintosh = 1,

            ISO = 2,

            Windows = 3,

            Custom = 4
        }

        internal enum FontNameId : byte
        {
            CopyrightNotice = 0,
            FamilyName = 1,

            //  The name of the style. Bold
            SubfamilyName = 2,

            UniqueIdentifier = 3,
            FullName = 4,
            VersionString = 5,
            PostScriptName = 6,
            Trademark = 7,
            ManufacturerName = 8,
            Designer = 9,
            Description = 10,
            URLVendor = 11,
            URLDesigner = 12,
            LicenseDescription = 13,
            URLLicense = 14,
            Reserved = 15,

            // If name ID 16 is absent, then name ID 1 is considered to be the typographic family name
            PreferredFamilyName = 16,

            // If it is absent, then name ID 2 is considered to be the typographic subfamily name
            PreferredSubfamily = 17,

            // Macintosh only
            CompatibleFullName = 18,

            SampleText = 19,
            PostScriptCID = 20,
            WWSFamilyName = 21,
            WWSSubfamilyName = 22,
            LightBackgroundPalette = 23,
            DarkBackgroundPalette = 24,
            VariationsPostScriptNamePrefix = 25
        }

        private static HeadTable ParseHeadTable(Dictionary<string, OffsetItem> tables, PureStreamReader sr)
        {
            if (!tables.TryGetValue("head", out var tl))
            {
                throw new Exception($"Table 'head' does not exist");
            }

            // Ignored majorVersion(2), minorVersion(2), fontRevision(4), checksumAdjustment(4), magicNumber(4)
            sr.Seek(tl.Offset + 16);

            var flags = sr.ReadUshort();
            var unitsPerEm = sr.ReadUshort();

            // Ignored created & modified fields (8 bytes x 2)
            sr.Skip(16);

            var xMin = sr.ReadShort();
            var yMin = sr.ReadShort();
            var xMax = sr.ReadShort();
            var yMax = sr.ReadShort();
            var macStyle = sr.ReadUshort();

            // Ignored lowestRecPPEM and fontDirectionHint
            sr.Skip(4);

            var indexToLocFormat = sr.ReadShort();

            return new HeadTable
            {
                flags = flags,
                unitsPerEm = unitsPerEm,
                xMin = xMin,
                yMin = yMin,
                xMax = xMax,
                yMax = yMax,
                macStyle = macStyle,
                indexToLocFormat = indexToLocFormat
            };
        }

        private static HheaTable ParseHheaTable(Dictionary<string, OffsetItem> tables, PureStreamReader sr)
        {
            if (!tables.TryGetValue(HHEA, out var tl))
            {
                throw new Exception($"Table '{HHEA}' does not exist");
            }

            // Ignored majorVersion(2) & minorVersion(2)
            sr.Seek(tl.Offset + 4);

            var ascender = sr.ReadShort();
            var descender = sr.ReadShort();
            var lineGap = sr.ReadShort();
            var advanceWidthMax = sr.ReadUshort();
            var minLeftSideBearing = sr.ReadShort();
            var minRightSideBearing = sr.ReadShort();
            var xMaxExtent = sr.ReadShort();
            var caretSlopeRise = sr.ReadShort();
            var caretSlopeRun = sr.ReadShort();

            // Ignored caretOffset(2), reserved fields(2 x 4), metricDataFormat(2)
            sr.Skip(12);

            var numberOfHMetrics = sr.ReadUshort();

            return new HheaTable
            {
                ascender = ascender,
                descender = descender,
                lineGap = lineGap,
                advanceWidthMax = advanceWidthMax,
                minLeftSideBearing = minLeftSideBearing,
                minRightSideBearing = minRightSideBearing,
                xMaxExtent = xMaxExtent,
                caretSlopeRise = caretSlopeRise,
                caretSlopeRun = caretSlopeRun,
                numberOfHMetrics = numberOfHMetrics
            };
        }

        private static VheaTable? ParseVheaTable(Dictionary<string, OffsetItem> tables, PureStreamReader sr)
        {
            if (!tables.TryGetValue("vhea", out var tl))
            {
                // Table 'vhea' does not exist
                return null;
            }

            // Ignored Version16Dot16(4)
            sr.Seek(tl.Offset + 4);

            var vertTypoAscender = sr.ReadShort();
            var vertTypoDescender = sr.ReadShort();
            var vertTypoLineGap = sr.ReadShort();
            var advanceHeightMax = sr.ReadUshort();
            var minTopSideBearing = sr.ReadShort();
            var minBottomSideBearing = sr.ReadShort();
            var yMaxExtent = sr.ReadShort();
            var caretSlopeRise = sr.ReadShort();
            var caretSlopeRun = sr.ReadShort();

            // Ignored caretOffset(2), reserved fields(2 x 4), metricDataFormat(2)
            sr.Skip(12);

            var numOfLongVerMetrics = sr.ReadUshort();

            return new VheaTable
            {
                vertTypoAscender = vertTypoAscender,
                vertTypoDescender = vertTypoDescender,
                vertTypoLineGap = vertTypoLineGap,
                advanceHeightMax = advanceHeightMax,
                minTopSideBearing = minTopSideBearing,
                minBottomSideBearing = minBottomSideBearing,
                yMaxExtent = yMaxExtent,
                caretSlopeRise = caretSlopeRise,
                caretSlopeRun = caretSlopeRun,
                numOfLongVerMetrics = numOfLongVerMetrics
            };
        }

        private static MetricsTable ParseMetricsTable(Dictionary<string, OffsetItem> tables, PureStreamReader sr, HeadTable head, HheaTable hhea)
        {
            if (tables.TryGetValue("OS/2", out var tl))
            {
                sr.Seek(tl.Offset);

                var version = sr.ReadUshort();
                var xAvgCharWidth = sr.ReadShort();
                var usWeightClass = sr.ReadUshort();
                var usWidthClass = sr.ReadUshort();
                var fsType = sr.ReadShort();
                var ySubscriptXSize = sr.ReadShort();
                var ySubscriptYSize = sr.ReadShort();
                var ySubscriptXOffset = sr.ReadShort();
                var ySubscriptYOffset = sr.ReadShort();
                var ySuperscriptXSize = sr.ReadShort();
                var ySuperscriptYSize = sr.ReadShort();
                var ySuperscriptXOffset = sr.ReadShort();
                var ySuperscriptYOffset = sr.ReadShort();
                var yStrikeoutSize = sr.ReadShort();
                var yStrikeoutPosition = sr.ReadShort();
                var sFamilyClass = sr.ReadShort();
                var panose = sr.ReadBytes(10).ToArray();
                sr.Skip(16);
                var achVendID = sr.ReadBytes(4).ToArray();
                var fsSelection = sr.ReadUshort();
                var usFirstCharIndex = sr.ReadUshort();
                var usLastCharIndex = sr.ReadUshort();
                var sTypoAscender = sr.ReadShort();
                var sTypoDescender = sr.ReadShort();
                if (sTypoDescender > 0)
                    sTypoDescender = (short)(-sTypoDescender);
                var sTypoLineGap = sr.ReadShort();
                var usWinAscent = sr.ReadUshort();
                var usWinDescent = sr.ReadUshort();
                var ulCodePageRange1 = 0;
                var ulCodePageRange2 = 0;
                if (version > 0)
                {
                    ulCodePageRange1 = sr.ReadInt();
                    ulCodePageRange2 = sr.ReadInt();
                }
                short sCapHeight;
                if (version > 1)
                {
                    sr.Skip(2);
                    sCapHeight = sr.ReadShort();
                }
                else
                    sCapHeight = (short)(0.7 * head.unitsPerEm);

                return new MetricsTable
                {
                    xAvgCharWidth = xAvgCharWidth,
                    usWeightClass = usWeightClass,
                    usWidthClass = usWidthClass,
                    fsType = fsType,
                    ySubscriptXSize = ySubscriptXSize,
                    ySubscriptYSize = ySubscriptYSize,
                    ySubscriptXOffset = ySubscriptXOffset,
                    ySubscriptYOffset = ySubscriptYOffset,
                    ySuperscriptXSize = ySuperscriptXSize,
                    ySuperscriptYSize = ySuperscriptYSize,
                    ySuperscriptXOffset = ySuperscriptXOffset,
                    ySuperscriptYOffset = ySuperscriptYOffset,
                    yStrikeoutSize = yStrikeoutSize,
                    yStrikeoutPosition = yStrikeoutPosition,
                    sFamilyClass = sFamilyClass,
                    panose = panose,
                    achVendID = achVendID,
                    fsSelection = fsSelection,
                    usFirstCharIndex = usFirstCharIndex,
                    usLastCharIndex = usLastCharIndex,
                    sTypoAscender = sTypoAscender,
                    sTypoDescender = sTypoDescender,
                    sTypoLineGap = sTypoLineGap,
                    usWinAscent = usWinAscent,
                    usWinDescent = usWinDescent,
                    ulCodePageRange1 = ulCodePageRange1,
                    ulCodePageRange2 = ulCodePageRange2,
                    sCapHeight = sCapHeight
                };
            }
            else
            {
                int usWeightClass;
                int usWidthClass;
                if (head.macStyle == 0)
                {
                    usWeightClass = 700;
                    usWidthClass = 5;
                }
                else if (head.macStyle == 5)
                {
                    usWeightClass = 400;
                    usWidthClass = 3;
                }
                else if (head.macStyle == 6)
                {
                    usWeightClass = 400;
                    usWidthClass = 7;
                }
                else
                {
                    usWeightClass = 400;
                    usWidthClass = 5;
                }

                return new MetricsTable
                {
                    usWeightClass = usWeightClass,
                    usWidthClass = usWidthClass,
                    sTypoAscender = (short)(hhea.ascender - 0.21 * hhea.ascender),
                    sTypoDescender = (short)-(Math.Abs(hhea.descender) - Math.Abs(hhea.descender) * 0.07),
                    sTypoLineGap = (short)(hhea.lineGap * 2),
                    usWinAscent = hhea.ascender,
                    usWinDescent = hhea.descender,
                    sCapHeight = (short)(0.7 * head.unitsPerEm)
                };
            }
        }

        private static PostTable ParsePostTable(Dictionary<string, OffsetItem> tables, PureStreamReader sr, HheaTable hhea)
        {
            double italicAngle;
            bool isFixedPitch;
            short underlinePosition;
            short underlineThickness;
            if (tables.TryGetValue("post", out var tl))
            {
                sr.Seek(tl.Offset + 4);
                var mantissa = sr.ReadShort();
                var fraction = sr.ReadUshort();
                italicAngle = mantissa + fraction/16384.0d;
                underlinePosition = sr.ReadShort();
                underlineThickness = sr.ReadShort();
                isFixedPitch = sr.ReadInt() != 0;
            }
            else
            {
                italicAngle = -Math.Atan2(hhea.caretSlopeRun, hhea.caretSlopeRise)*180/Math.PI;
                underlinePosition = 0;
                underlineThickness = 0;
                isFixedPitch = true;
            }

            int numGlyphs;
            if (!tables.TryGetValue("maxp", out tl))
            {
                numGlyphs = 65536;
            }
            else
            {
                // Ignored Version16Dot16
                sr.Seek(tl.Offset + 4);

                // numGlyphs - The number of glyphs in the font
                numGlyphs = sr.ReadUshort();
            }

            return new PostTable
            {
                italicAngle = italicAngle,
                underlinePosition = underlinePosition,
                underlineThickness = underlineThickness,
                isFixedPitch = isFixedPitch,
                numGlyphs = numGlyphs
            };
        }

        // hmtx — Horizontal Metrics Table
        // https://docs.microsoft.com/en-us/typography/opentype/spec/hmtx
        private static HMetrics[] ParseHmtxTable(Dictionary<string, OffsetItem> tables, PureStreamReader sr, int numberOfHMetrics, int unitsPerEm)
        {
            if (!tables.TryGetValue(HMTX, out var tl))
            {
                throw new Exception($"Table '{HMTX}' does not exist");
            }

            //  As an optimization, the number of records can be less than the number of glyphs,
            //  in which case the advance width value of the last record applies to all remaining glyph IDs.
            var hmetrics = new HMetrics[numberOfHMetrics];

            sr.Seek(tl.Offset);

            for (int k = 0; k < numberOfHMetrics; ++k)
            {
                var advanceWidth = sr.ReadUshort();
                var lsb = sr.ReadShort();

                hmetrics[k] = new HMetrics
                {
                    advanceWidth = (ushort)(advanceWidth * 1000 / unitsPerEm),
                    lsb = (short)(lsb * 1000 / unitsPerEm)
                };
            }

            return hmetrics;
        }

        // vmtx — Vertical Metrics Table
        // https://docs.microsoft.com/en-us/typography/opentype/spec/vmtx
        private static VMetrics[]? ParseVmtxTable(Dictionary<string, OffsetItem> tables, PureStreamReader sr, int numOfLongVerMetrics)
        {
            if (!tables.TryGetValue("vmtx", out var tl))
            {
                // Table 'vmtx' does not exist
                return null;
            }

            //  As an optimization, the number of records can be less than the number of glyphs,
            var vmetrics = new VMetrics[numOfLongVerMetrics];

            sr.Seek(tl.Offset);

            for (int k = 0; k < numOfLongVerMetrics; ++k)
            {
                var advanceHeight = sr.ReadUshort();
                var topSideBearing = sr.ReadShort();

                vmetrics[k] = new VMetrics
                {
                    advanceHeight = advanceHeight,
                    topSideBearing = topSideBearing
                };
            }

            return vmetrics;
        }

        private static FontNamePlatform GetPlatform(int platformID)
        {
            if (!Enum.TryParse<FontNamePlatform>(platformID.ToString(), out var platform))
            {
                platform = FontNamePlatform.Custom;
            }
            return platform;
        }

        // name - Naming Table
        // https://docs.microsoft.com/en-us/typography/opentype/spec/name
        private static FontName[] ParseNameTable(Dictionary<string, OffsetItem> tables, PureStreamReader sr)
        {
            if (!tables.TryGetValue("name", out var tl))
            {
                throw new Exception($"Table 'name' does not exist");
            }

            sr.Seek(tl.Offset);

            var version = sr.ReadUshort();
            var count = sr.ReadUshort();
            var storageOffset = sr.ReadUshort();

            var names = new FontName[count];

            for (var k = 0; k < count; k++)
            {
                // https://docs.microsoft.com/en-us/typography/opentype/spec/cmap#encoding-records-and-encodings

                var platform = GetPlatform(sr.ReadUshort());

                var encodingID = sr.ReadUshort();
                var languageID = sr.ReadUshort();

                var nameID = sr.ReadUshort();
                if (!Enum.TryParse<FontNameId>(nameID.ToString(), out var nameIdValue))
                {
                    nameIdValue = FontNameId.Reserved;
                }

                var length = sr.ReadUshort();
                var offset = sr.ReadUshort();

                // Current position
                var pos = (int)sr.CurrentPosition;

                // Seek string position
                sr.Seek(tl.Offset + storageOffset + offset);

                string name;
                if (platform == FontNamePlatform.Unicode || (platform == FontNamePlatform.Macintosh && encodingID > 0) || platform == FontNamePlatform.Windows || (platform == FontNamePlatform.ISO && encodingID == 1))
                {
                    // All string data for platform 3 (windows) must be encoded in UTF-16BE
                    // Platform = ISO, encoding id = 0 (7-bit ASCII), 1 = ISO 10646, 2 = ISO 8859-1
                    name = sr.ReadString(length, Encoding.BigEndianUnicode);
                }
                else
                {
                    name = sr.ReadString(length, Encoding1252);
                }

                // Back to previous reading position
                sr.Seek(pos);

                names[k] = new FontName
                {
                    Platform = platform,
                    EncodingID = encodingID,
                    LanguageID = languageID,
                    NameId = nameIdValue,
                    Name = name
                };
            }

            return names;
        }

        // The kerning table contains the values that control the inter-character spacing for the glyphs in a font
        // https://docs.microsoft.com/en-us/typography/opentype/spec/kern
        private static Dictionary<int, float>? ParseKernTable(Dictionary<string, OffsetItem> tables, PureStreamReader sr, int unitsPerEm)
        {
            if (!tables.TryGetValue("kern", out var tl))
                return null;

            // Ignored version field
            sr.Seek(tl.Offset + 2);

            // Number of subtables in the kerning table
            var nTables = sr.ReadUshort();

            // subtables start position (version + nTables)
            var checkpoint = tl.Offset + 4;

            // Last subtable length in byte
            ushort length = 0;

            // Kerning
            var kern = new Dictionary<int, float>();

            for (var k = 0; k < nTables; k++)
            {
                checkpoint += length;
                sr.Seek(checkpoint);

                // Kerning subtables will share the same header format.
                // This header is used to identify the format of the subtable and the kind of information it contains

                // Ignored Kern subtable version number
                sr.Skip(2);

                // Length of the subtable, in bytes (including this header)
                length = sr.ReadUshort();

                // What type of information is contained in this table
                var coverage = sr.ReadUshort();

                if ((coverage & 0xfff7) == 0x0001)
                {
                    // Format 0 is the only subtable format supported by Windows

                    // This gives the number of kerning pairs in the table
                    var nPairs = sr.ReadUshort();

                    // Ignored searchRange, entrySelector, and rangeShift fields (2 x 3)
                    sr.Skip(6);

                    for (var j = 0; j < nPairs; ++j)
                    {
                        // The glyph index for the left-hand and right-hand glyph in the kerning pair
                        // Read left and right fields at the same time
                        var pair = sr.ReadInt();

                        // The kerning value for the above pair, in FUnits
                        //  If this value is greater than zero, the characters will be moved apart.
                        //  If this value is less than zero, the character will be moved closer together
                        var value = sr.ReadShort() * 1000f / unitsPerEm;
                        kern[pair] = value;
                    }
                }
            }

            return kern;
        }

        // cmap - Character to Glyph Index Mapping Table
        // It may contain more than one subtable, in order to support more than one character encoding scheme
        // https://docs.microsoft.com/en-us/typography/opentype/spec/cmap
        private static CMapFormat[] ParseCMapTable(Dictionary<string, OffsetItem> tables, PureStreamReader sr)
        {
            if (!tables.TryGetValue("cmap", out var tl))
            {
                throw new Exception($"Table 'cmap' does not exist");
            }

            sr.Seek(tl.Offset);

            // Ignored version field
            sr.Skip(2);

            // All formats
            var formats = new List<CMapFormat>();

            // The table header indicates the character encodings for which subtables are present
            var numTables = sr.ReadUshort();
            for (var k = 0; k < numTables; k++)
            {
                var platform = GetPlatform(sr.ReadUshort());
                var encodingID = sr.ReadUshort();
                var offset = sr.ReadInt();

                // Current position
                var pos = sr.CurrentPosition;

                sr.Seek(tl.Offset + offset);

                var format = sr.ReadUshort();

                if (format == 0)
                {
                    // Ignored length field
                    sr.Skip(2);

                    var language = sr.ReadUshort();

                    Dictionary<int, int> h0 = [];
                    for (var f = 0; f < 256; f++)
                    {
                        // An array that maps character codes to glyph index values
                        // index is character code, value is glyph id
                        h0[f] = sr.ReadSbyte();
                    }

                    formats.Add(new CMapFormat
                    {
                        Format = format,
                        Platform = platform,
                        EncodingID = encodingID,
                        LanguageID = language,
                        Glyphs = h0
                    });
                }
                else if (format == 4)
                {
                    var length = sr.ReadUshort();
                    var language = sr.ReadUshort();

                    // 2 × segCount
                    var segCount = sr.ReadUshort() / 2;

                    // Ignored searchRange, entrySelector, and rangeShift fields
                    sr.Skip(6);

                    // End characterCode for each segment, last=0xFFFF
                    var endCode = new int[segCount];
                    for (var c = 0; c < segCount; c++)
                    {
                        endCode[c] = sr.ReadUshort();
                    }

                    // Ignored reservedPad field
                    sr.Skip(2);

                    // Start character code for each segment
                    var startCode = new int[segCount];
                    for (var c = 0; c < segCount; c++)
                    {
                        startCode[c] = sr.ReadUshort();
                    }

                    // Delta for all character codes in segment
                    var idDelta = new int[segCount];
                    for (var c = 0; c < segCount; c++)
                    {
                        idDelta[c] = sr.ReadUshort();
                    }

                    // idRangeOffsets - Offsets into glyphIdArray or 0
                    var idRO = new int[segCount];
                    for (var c = 0; c < segCount; c++)
                    {
                        idRO[c] = sr.ReadUshort();
                    }

                    // glyphIdArray
                    var glyphId = new int[length / 2 - 8 - segCount * 4];
                    for (var c = 0; c < glyphId.Length; c++)
                    {
                        glyphId[c] = sr.ReadUshort();
                    }

                    Dictionary<int, int> h4 = [];
                    for (var c = 0; c < segCount; c++)
                    {
                        for (var j = startCode[c]; j <= endCode[c] && j != 0xFFFF; j++)
                        {
                            int glyph;
                            if (idRO[c] == 0)
                            {
                                glyph = (j + idDelta[c]) & 0xFFFF;
                            }
                            else
                            {
                                var idx = c + idRO[c] / 2 - segCount + j - startCode[c];
                                if (idx >= glyphId.Length)
                                    continue;
                                glyph = (glyphId[idx] + idDelta[c]) & 0xFFFF;
                            }
                            h4[j] = glyph;
                        }
                    }

                    formats.Add(new CMapFormat
                    {
                        Format = format,
                        Platform = platform,
                        EncodingID = encodingID,
                        LanguageID = language,
                        Glyphs = h4
                    });
                }
                else if (format == 6)
                {
                    // Ignored length field
                    sr.Skip(2);

                    var language = sr.ReadUshort();
                    var firstCode = sr.ReadUshort();
                    var entryCount = sr.ReadUshort();

                    Dictionary<int, int> h6 = [];

                    for (var c = 0; c < entryCount; c++)
                    {
                        h6[c + firstCode] = sr.ReadUshort();
                    }

                    formats.Add(new CMapFormat
                    {
                        Format = format,
                        Platform = platform,
                        EncodingID = encodingID,
                        LanguageID = language,
                        Glyphs = h6
                    });
                }
                else if (format == 12)
                {
                    // Ignored reserved(2) and length(4) fields
                    sr.Skip(6);

                    var language = sr.ReadUshort();

                    Dictionary<int, int> h12 = [];

                    var numGroups = sr.ReadInt();
                    for (var c = 0; c < numGroups; c++)
                    {
                        var startCharCode = sr.ReadInt();
                        var endCharCode = sr.ReadInt();
                        var startGlyphID = sr.ReadInt();
                        for (var i = startCharCode; i <= endCharCode; i++)
                        {
                            h12[i] = startGlyphID;
                            startGlyphID++;
                        }
                    }

                    formats.Add(new CMapFormat
                    {
                        Format = format,
                        Platform = platform,
                        EncodingID = encodingID,
                        LanguageID = language,
                        Glyphs = h12
                    });
                }

                // Back to previous position
                sr.Seek(pos);
            }

            return [.. formats];
        }

        private static int[] ParseLocaTable(Dictionary<string, OffsetItem> tables, PureStreamReader sr, bool shortVersion)
        {
            if (!tables.TryGetValue(LOCA, out var tl))
            {
                throw new Exception($"Table '{LOCA}' does not exist");
            }

            sr.Seek(tl.Offset);

            int[] locaTable;

            if (shortVersion)
            {
                var entries = tl.Length / 2;
                locaTable = new int[entries];
                for (var k = 0; k < entries; ++k)
                    locaTable[k] = sr.ReadUshort() * 2;
            }
            else
            {
                var entries = tl.Length / 4;
                locaTable = new int[entries];
                for (var k = 0; k < entries; ++k)
                    locaTable[k] = sr.ReadInt();
            }

            return locaTable;
        }
    }
}
