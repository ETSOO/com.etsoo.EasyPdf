using com.etsoo.EasyPdf.IO;
using CommunityToolkit.HighPerformance;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace com.etsoo.EasyPdf.Font
{
    /// <summary>
    /// Font parser
    /// 字体解析
    /// </summary>
    public class EasyFont
    {
        /// <summary>
        /// '\0'
        /// </summary>
        private const byte NullByte = 0;
        private const string TTCF = "ttcf";

        private const string CMAP = "cmap";
        private const string FPGM = "fpgm";
        private const string GLYF = "glyf";
        private const string HEAD = "head";
        private const string HHEA = "hhea";
        private const string HMTX = "hmtx";
        private const string LOCA = "loca";
        private const string MAXP = "maxp";
        private const string NAME = "name";
        private const string OS2 = "OS/2";
        private const string POST = "post";
        private const string PREP = "prep";

        // Tables to deal
        // https://learn.microsoft.com/en-us/typography/opentype/spec/otff#font-tables
        private static readonly string[] requiredTables = {
            CMAP, FPGM, GLYF, HEAD, HHEA, HMTX, LOCA, MAXP, NAME, OS2, POST, PREP
        };

        /// <summary>
        /// Create font subset
        /// 创建字体子集
        /// </summary>
        /// <param name="fontStream">Font stream</param>
        /// <param name="includedChars">Included characters</param>
        /// <param name="familyName">Specific family name (TTC file contains multiple fonts), default is the first font</param>
        /// <returns>Result</returns>
        public static async Task<MemoryStream> CreateSubsetAsync(Stream fontStream, IEnumerable<char> includedChars, string? familyName = null)
        {
            // Unique characters
            includedChars = includedChars.Distinct();

            return await Task.Run(async () =>
            {
                // Output stream
                var stream = IOUtils.StreamManager.GetStream();

                // Stream accessor
                using var rf = new PdfRandomAccessor(fontStream);

                // https://docs.microsoft.com/en-us/typography/opentype/spec/otff#ttc-header
                // ttcTag
                var mainTag = rf.ReadString(4);
                if (mainTag.Equals(TTCF))
                {
                    // Read majorVersion, and minorVersion fields
                    // rf.SkipBytes(4);
                    var versionBytes = new byte[4];
                    rf.ReadFully(versionBytes);

                    // Fonts under the family
                    var numFonts = rf.ReadInt();

                    /*
                    // Output TTCF
                    stream.Write(Encoding.ASCII.GetBytes(TTCF));
                    stream.Write(versionBytes);
                    stream.Write(GetBytes(numFonts));

                    // Hold current position
                    var pos = stream.Position;

                    // Array of offsets to the TableDirectory for each font from the beginning of the file
                    var offsets = new int[numFonts];
                    for (var f = 0; f < numFonts; f++)
                    {
                        offsets[f] = rf.ReadInt();

                        // Take position bytes
                        stream.Write(GetBytes(0));
                    }

                    var newOffsets = new int[numFonts];

                    // Parse fonts
                    for (var f = 0; f < numFonts; f++)
                    {
                        var offset = offsets[f];

                        rf.Seek(offset);
                        var font = ParseFont(rf);

                        newOffsets[f] = (int)stream.Length;

                        await font.ToSubsetAsync(stream, includedChars);
                    }

                    // Update offset
                    stream.Position = pos;
                    foreach (var f in newOffsets)
                    {
                        stream.Write(GetBytes(f));
                    }
                    */

                    // Array of offsets to the TableDirectory for each font from the beginning of the file
                    var offsets = new int[numFonts];
                    for (var f = 0; f < numFonts; f++)
                    {
                        offsets[f] = rf.ReadInt();
                    }

                    // Parse fonts
                    for (var f = 0; f < numFonts; f++)
                    {
                        var offset = offsets[f];

                        rf.Seek(offset);
                        var font = ParseFont(rf, familyName);
                        if (font != null)
                        {
                            await font.ToSubsetAsync(stream, includedChars);
                            break;
                        }
                    }
                }
                else
                {
                    rf.Seek(0);
                    var font = ParseFont(rf, familyName);
                    if (font != null)
                        await font.ToSubsetAsync(stream, includedChars);
                }

                rf.Close();

                stream.Position = 0;
                return stream;
            });
        }

        private static EasyFont? ParseFont(PdfRandomAccessor rf, string? familyName = null)
        {
            // sfntVersion
            // 0x00010000 or 0x4F54544F ('OTTO')
            var ttId = rf.ReadInt();
            if (ttId != 0x00010000 && ttId != 0x4F54544F)
                throw new Exception($"It is not a valid ttf or otf stream");

            // Contains the location of the several tables
            Dictionary<string, OffsetItem> tables = new Dictionary<string, OffsetItem>();

            // Number of tables
            var tableCount = rf.ReadUnsignedShort();

            // Ignored searchRange, entrySelector, rangeShift fields (2 x 3)
            rf.SkipBytes(6);

            for (var k = 0; k < tableCount; ++k)
            {
                // Table identifier
                var tag = rf.ReadString(4);

                // Checksum for this table
                // Offset from beginning of font file
                // Length of this table
                tables[tag] = new OffsetItem(rf.ReadUInt(), rf.ReadUInt(), rf.ReadUInt());
            }

            // Parse names
            if (!string.IsNullOrEmpty(familyName))
            {
                var names = ParseNameTable(tables, rf);
                if (!names.Any(
                    n => (n.NameId == FontNameId.PostScriptName || n.NameId == FontNameId.FullName)
                    && n.Name.Equals(familyName, StringComparison.OrdinalIgnoreCase)
                  )
                ) return null;
            }

            var head = ParseHeadTable(tables, rf);
            var locas = ParseLocaTable(tables, rf, head.LocaShortVersion);
            var cmaps = ParseCMapTable(tables, rf);

            // Fill required tables data, 1 for LOCA
            ushort filledTables = 1;
            foreach (var table in requiredTables)
            {
                if (table == LOCA || !tables.TryGetValue(table, out var td))
                {
                    continue;
                }

                // Move to the start
                rf.Seek(td.Offset);

                // Read all data
                var data = new byte[td.Length];
                rf.ReadFully(data);
                td.Data = data;

                filledTables++;
            }

            return new EasyFont(tables, filledTables, head, locas, cmaps);
        }

        private static HeadTable ParseHeadTable(Dictionary<string, OffsetItem> tables, PdfRandomAccessor rf)
        {
            if (!tables.TryGetValue("head", out var tl))
            {
                throw new Exception($"Table 'head' does not exist");
            }

            // Ignored majorVersion(2), minorVersion(2), fontRevision(4), checksumAdjustment(4), magicNumber(4)
            rf.Seek(tl.Offset + 16);

            var flags = rf.ReadUnsignedShort();
            var unitsPerEm = rf.ReadUnsignedShort();

            // Ignored created & modified fields (8 bytes x 2)
            rf.SkipBytes(16);

            var xMin = rf.ReadShort();
            var yMin = rf.ReadShort();
            var xMax = rf.ReadShort();
            var yMax = rf.ReadShort();
            var macStyle = rf.ReadUnsignedShort();

            // Ignored lowestRecPPEM and fontDirectionHint
            rf.SkipBytes(4);

            var indexToLocFormat = rf.ReadShort();

            return new HeadTable
            {
                Flags = flags,
                UnitsPerEm = unitsPerEm,
                XMin = xMin,
                YMin = yMin,
                XMax = xMax,
                YMax = yMax,
                MacStyle = macStyle,
                IndexToLocFormat = indexToLocFormat
            };
        }

        private static FontNamePlatform GetPlatform(int platformID)
        {
            if (!Enum.TryParse<FontNamePlatform>(platformID.ToString(), out var platform))
            {
                platform = FontNamePlatform.Custom;
            }
            return platform;
        }

        // cmap - Character to Glyph Index Mapping Table
        // It may contain more than one subtable, in order to support more than one character encoding scheme
        // https://docs.microsoft.com/en-us/typography/opentype/spec/cmap
        private static CMapFormat[] ParseCMapTable(Dictionary<string, OffsetItem> tables, PdfRandomAccessor rf)
        {
            if (!tables.TryGetValue("cmap", out var tl))
            {
                throw new Exception($"Table 'cmap' does not exist");
            }

            rf.Seek(tl.Offset);

            // Ignored version field
            rf.SkipBytes(2);

            // All formats
            var formats = new List<CMapFormat>();

            // The table header indicates the character encodings for which subtables are present
            var numTables = rf.ReadUnsignedShort();
            for (var k = 0; k < numTables; k++)
            {
                var platform = GetPlatform(rf.ReadUnsignedShort());
                var encodingID = rf.ReadUnsignedShort();
                var offset = rf.ReadInt();

                // Current position
                var pos = rf.FilePointer;

                rf.Seek(tl.Offset + offset);

                var format = rf.ReadUnsignedShort();

                if (format == 0)
                {
                    // Ignored length field
                    rf.SkipBytes(2);

                    var language = rf.ReadUnsignedShort();

                    var h0 = new Dictionary<int, int>();
                    for (var f = 0; f < 256; f++)
                    {
                        // An array that maps character codes to glyph index values
                        // index is character code, value is glyph id
                        h0[f] = rf.ReadUnsignedByte();
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
                    var length = rf.ReadUnsignedShort();
                    var language = rf.ReadUnsignedShort();

                    // 2 × segCount
                    var segCount = rf.ReadUnsignedShort() / 2;

                    // Ignored searchRange, entrySelector, and rangeShift fields
                    rf.SkipBytes(6);

                    // End characterCode for each segment, last=0xFFFF
                    var endCode = new int[segCount];
                    for (var c = 0; c < segCount; c++)
                    {
                        endCode[c] = rf.ReadUnsignedShort();
                    }

                    // Ignored reservedPad field
                    rf.SkipBytes(2);

                    // Start character code for each segment
                    var startCode = new int[segCount];
                    for (var c = 0; c < segCount; c++)
                    {
                        startCode[c] = rf.ReadUnsignedShort();
                    }

                    // Delta for all character codes in segment
                    var idDelta = new int[segCount];
                    for (var c = 0; c < segCount; c++)
                    {
                        idDelta[c] = rf.ReadUnsignedShort();
                    }

                    // idRangeOffsets - Offsets into glyphIdArray or 0
                    var idRO = new int[segCount];
                    for (var c = 0; c < segCount; c++)
                    {
                        idRO[c] = rf.ReadUnsignedShort();
                    }

                    // glyphIdArray
                    var glyphId = new int[length / 2 - 8 - segCount * 4];
                    for (var c = 0; c < glyphId.Length; c++)
                    {
                        glyphId[c] = rf.ReadUnsignedShort();
                    }

                    var h4 = new Dictionary<int, int>();
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
                    rf.SkipBytes(2);

                    var language = rf.ReadUnsignedShort();
                    var firstCode = rf.ReadUnsignedShort();
                    var entryCount = rf.ReadUnsignedShort();

                    var h6 = new Dictionary<int, int>();

                    for (var c = 0; c < entryCount; c++)
                    {
                        h6[c + firstCode] = rf.ReadUnsignedShort();
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
                    rf.SkipBytes(6);

                    var language = rf.ReadUnsignedShort();

                    var h12 = new Dictionary<int, int>();

                    var numGroups = rf.ReadInt();
                    for (var c = 0; c < numGroups; c++)
                    {
                        var startCharCode = rf.ReadInt();
                        var endCharCode = rf.ReadInt();
                        var startGlyphID = rf.ReadInt();
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
                rf.Seek(pos);
            }

            return formats.ToArray();
        }

        private static int[] ParseLocaTable(Dictionary<string, OffsetItem> tables, PdfRandomAccessor rf, bool shortVersion)
        {
            if (!tables.TryGetValue(LOCA, out var tl))
            {
                throw new Exception($"Table '{LOCA}' does not exist");
            }

            rf.Seek(tl.Offset);

            int[] locaTable;

            if (shortVersion)
            {
                var entries = tl.Length / 2;
                locaTable = new int[entries];
                for (var k = 0; k < entries; ++k)
                    locaTable[k] = rf.ReadUnsignedShort() * 2;
            }
            else
            {
                var entries = tl.Length / 4;
                locaTable = new int[entries];
                for (var k = 0; k < entries; ++k)
                    locaTable[k] = rf.ReadInt();
            }

            return locaTable;
        }

        // name - Naming Table
        // https://docs.microsoft.com/en-us/typography/opentype/spec/name
        private static FontName[] ParseNameTable(Dictionary<string, OffsetItem> tables, PdfRandomAccessor rf)
        {
            if (!tables.TryGetValue("name", out var tl))
            {
                throw new Exception($"Table 'name' does not exist");
            }

            rf.Seek(tl.Offset);

            var version = rf.ReadUnsignedShort();
            var count = rf.ReadUnsignedShort();
            var storageOffset = rf.ReadUnsignedShort();

            var names = new FontName[count];

            for (var k = 0; k < count; k++)
            {
                // https://docs.microsoft.com/en-us/typography/opentype/spec/cmap#encoding-records-and-encodings

                var platform = GetPlatform(rf.ReadUnsignedShort());

                var encodingID = rf.ReadUnsignedShort();
                var languageID = rf.ReadUnsignedShort();

                var nameID = rf.ReadUnsignedShort();
                if (!Enum.TryParse<FontNameId>(nameID.ToString(), out var nameIdValue))
                {
                    nameIdValue = FontNameId.Reserved;
                }

                var length = rf.ReadUnsignedShort();
                var offset = rf.ReadUnsignedShort();

                // Current position
                var pos = (int)rf.FilePointer;

                // Seek string position
                rf.Seek(tl.Offset + storageOffset + offset);

                string name;
                if (platform == FontNamePlatform.Unicode || (platform == FontNamePlatform.Macintosh && encodingID > 0) || platform == FontNamePlatform.Windows || (platform == FontNamePlatform.ISO && encodingID == 1))
                {
                    // All string data for platform 3 (windows) must be encoded in UTF-16BE
                    // Platform = ISO, encoding id = 0 (7-bit ASCII), 1 = ISO 10646, 2 = ISO 8859-1
                    name = rf.ReadUnicodeString(length);
                }
                else
                {
                    name = rf.ReadString(length);
                }

                // Back to previous reading position
                rf.Seek(pos);

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

        private static byte[] GetBytes(short n)
        {
            return new[] { (byte)(n >> 8), (byte)n };
        }

        private static byte[] GetBytes(ushort n)
        {
            return new[] { (byte)(n >> 8), (byte)n };
        }

        private static byte[] GetBytes(int n)
        {
            return new byte[] { (byte)(n >> 24), (byte)(n >> 16), (byte)(n >> 8), (byte)n };
        }

        private static byte[] GetBytes(uint n)
        {
            return new byte[] { (byte)(n >> 24), (byte)(n >> 16), (byte)(n >> 8), (byte)n };
        }

        /// <summary>
        /// Table Directory
        /// </summary>
        readonly Dictionary<string, OffsetItem> Tables;

        /// <summary>
        /// Filled tables
        /// </summary>
        readonly ushort FilledTables;

        /// <summary>
        /// The content of table 'head'
        /// </summary>
        readonly HeadTable Head;

        /// <summary>
        /// The content of table 'loca'
        /// </summary>
        readonly int[] Locas;

        /// <summary>
        /// The content of table 'cmap'
        /// </summary>
        readonly CMapFormat[] CMaps;

        internal EasyFont(Dictionary<string, OffsetItem> tables, ushort filledTables, HeadTable head, int[] locas, CMapFormat[] cMaps)
        {
            Tables = tables;
            FilledTables = filledTables;
            Head = head;
            Locas= locas;
            CMaps = cMaps;
        }

        private void UpdateTableData(OffsetItem item, byte[] b)
        {
            item.Data = b;
            item.Length = (uint)b.Length;
            item.Checksum = CalculateChecksum(b);
        }

        private uint CalculateChecksum(byte[] b)
        {
            var len = b.Length / 4;
            uint v0 = 0;
            uint v1 = 0;
            uint v2 = 0;
            uint v3 = 0;
            var ptr = 0;
            for (var k = 0; k < len; k++)
            {
                v3 += (uint)b[ptr++] & 0xff;
                v2 += (uint)b[ptr++] & 0xff;
                v1 += (uint)b[ptr++] & 0xff;
                v0 += (uint)b[ptr++] & 0xff;
            }
            return v0 + (v1 << 8) + (v2 << 16) + (v3 << 24);
        }

        /// <summary>
        /// Get glyph id
        /// 获取字形编号
        /// </summary>
        /// <param name="cid">Character id</param>
        /// <returns>Glyph id</returns>
        private int GetGlyphId(char c)
        {
            var cid = (int)c;
            var items = CMaps.OrderByDescending(g => g.Format);

            foreach (var g in items)
            {
                if (g.Glyphs.TryGetValue(cid, out var id))
                {
                    return id;
                }
            }

            // Regardless of the encoding scheme, character codes that do not correspond to any glyph in the font
            // should be mapped to glyph index 0
            return 0;
        }

        /// <summary>
        /// To subset
        /// 生成子集
        /// </summary>
        /// <param name="aw">Subset stream</param>
        /// <param name="chars">Included chars</param>
        /// <returns>Result</returns>
        private async Task<MemoryStream> ToSubsetAsync(MemoryStream aw, IEnumerable<char> chars)
        {
            // Glyph blocks (Cmap for char to glyph id map)
            await using var newGlyfs = IOUtils.StreamManager.GetStream();

            // The indexToLoc table stores the offsets to the locations of the glyphs in the font,
            // relative to the beginning of the glyphData table, indexed by glyph ID
            await using var newLocas = IOUtils.StreamManager.GetStream();

            var glyfTable = Tables[GLYF];
            var glyfData = glyfTable.Data!;

            var glyhs = chars.Select(GetGlyphId).Distinct().OrderBy(g => g).ToArray();
            var glen = glyhs.Length;

            var locaLen = Locas.Length;

            var listGlyf = 0;

            var glyfPtr = 0;

            for (var k = 0; k < locaLen - 1; k++)
            {
                // Updated offset
                if (Head.LocaShortVersion)
                {
                    newLocas.Write(GetBytes((short)(glyfPtr / 2)));
                }
                else
                {
                    newLocas.Write(GetBytes(glyfPtr));
                }

                if (listGlyf < glen && glyhs[listGlyf] == k)
                {
                    listGlyf++;

                    var start = Locas[k];
                    var glyphLen = Locas[k + 1] - start;

                    // Write the specific glyph data
                    if (glyphLen > 0)
                    {
                        // Note: Some PostScript devices (and possibly other implementations) do not correctly render glyphs that have nested composite descriptions.
                        // A composite glyph description that has nested composites can be flattened to reference only simple glyphs as child components
                        // var numberOfContours = (short)((gData[0] << 8) + gData[1]);
                        // https://docs.microsoft.com/en-us/typography/opentype/spec/glyf
                        var gData = glyfData[start..(start + glyphLen)];
                        await newGlyfs.WriteAsync(gData);

                        glyfPtr += glyphLen;
                    }
                }
            }

            //  In order to compute the length of the last glyph element, there is an extra entry after the last valid index
            if (Head.LocaShortVersion)
            {
                newLocas.Write(GetBytes((short)(glyfPtr / 2)));
            }
            else
            {
                newLocas.Write(GetBytes(glyfPtr));
            }

            UpdateTableData(glyfTable, newGlyfs.ToArray());

            var locaTable = Tables[LOCA];
            UpdateTableData(locaTable, newLocas.ToArray());

            // Start writing
            var awLen = aw.Length;

            // Tag (4)
            aw.Write(GetBytes(0x00010000));

            // Tables (2)
            aw.Write(GetBytes(FilledTables));

            // 6 = 2 x 3, TableDirectory
            // https://learn.microsoft.com/en-us/typography/opentype/spec/otff
            var entrySelector = (ushort)Math.Floor(Math.Log(FilledTables, 2));
            var searchRange = (ushort)(Math.Pow(2, entrySelector) * 16);
            var rangeShift = (ushort)((FilledTables * 16) - searchRange);
            aw.Write(GetBytes(entrySelector));
            aw.Write(GetBytes(searchRange));
            aw.Write(GetBytes(rangeShift));

            // Offset, 16 = Table dictionary length
            var offset = (uint)(16 * FilledTables + 12 + awLen);

            foreach (var tableName in requiredTables)
            {
                if (!Tables.TryGetValue(tableName, out var table))
                {
                    continue;
                }

                // Write name (4)
                aw.Write(PdfRandomAccessor.Encoding1252.GetBytes(tableName));

                // Checksum (4)
                aw.Write(GetBytes(table.Checksum));

                // Offset (4)
                table.Offset = offset;
                aw.Write(GetBytes(offset));

                // Length (4)
                aw.Write(GetBytes(table.Length));

                // Next offset
                // (n + 3) & (~3) = (n + 3) - (n + 3) % 4
                // n = 1, 4; n = 2, 4, n = 5, 8
                offset += (uint)((table.Length + 3) & (~3));
            }

            var lastAdjust = 0u;
            foreach (var tableName in requiredTables)
            {
                if (!Tables.TryGetValue(tableName, out var table) || table.Data == null)
                {
                    continue;
                }

                for (var l = 0; l < lastAdjust; l++)
                {
                    aw.Write(NullByte);
                }

                await aw.WriteAsync(table.Data);

                var len = (uint)((table.Length + 3) & (~3));
                lastAdjust = len - table.Length;
            }

            return aw;
        }
    }
}