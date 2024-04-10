using com.etsoo.EasyPdf.Content;
using com.etsoo.EasyPdf.Objects;
using com.etsoo.EasyPdf.Support;
using com.etsoo.EasyPdf.Types;
using CommunityToolkit.HighPerformance;
using System.Buffers;
using System.Drawing;
using System.Text;

namespace com.etsoo.EasyPdf.Fonts
{
    /// <summary>
    /// PDF base font (TrueType)
    /// PDF 基础字体 (TrueType)
    /// </summary>
    internal partial class PdfBaseFont : IPdfBaseFont
    {
        // Minimum required tables for CID font
        private const string GLYF = "glyf";
        private const string LOCA = "loca";
        private const string MAXP = "maxp";
        private const string HMTX = "hmtx";
        private const string HHEA = "hhea";
        private static string[] tableNames = [
            "cmap", "name", "OS/2", "post", "cvt ", "fpgm", GLYF, "head", HHEA, HMTX, LOCA, MAXP, "prep"
        ];
        internal static int[] entrySelectors = [0, 0, 1, 1, 2, 2, 2, 2, 3, 3, 3, 3, 3, 3, 3, 3, 4, 4, 4, 4, 4];

        // Table Directory
        // https://docs.microsoft.com/en-us/typography/opentype/spec/otff
        internal static PdfBaseFont Parse(PdfRandomAccessFileOrArray rf)
        {
            // Contains the location of the several tables
            Dictionary<string, OffsetItem> tables = [];

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

            OffsetItem? cff = null;
            if (tables.TryGetValue("CFF ", out var tl))
            {
                cff = tl;
            }

            var head = ParseHeadTable(tables, rf);
            var hhea = ParseHheaTable(tables, rf);
            var vhea = ParseVheaTable(tables, rf);
            var metrics = ParseMetricsTable(tables, rf, head, hhea);
            var post = ParsePostTable(tables, rf, hhea);
            var locas = ParseLocaTable(tables, rf, head.locaShortVersion);
            var names = ParseNameTable(tables, rf);
            var cmaps = ParseCMapTable(tables, rf);
            var hmetrics = ParseHmtxTable(tables, rf, hhea.numberOfHMetrics, head.unitsPerEm);

            VMetrics[]? vmetrics = null;
            if (vhea is not null)
            {
                vmetrics = ParseVmtxTable(tables, rf, vhea.numOfLongVerMetrics);
            }

            var kern = ParseKernTable(tables, rf, head.unitsPerEm);

            // Fill required tables data
            ushort filledTables = 1;
            foreach (var table in tableNames)
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

            return new PdfBaseFont
            {
                Tables = tables,
                FilledTables = filledTables,
                Cff = cff,
                Head = head,
                Hhea = hhea,
                Vhea = vhea,
                Metrics = metrics,
                Post = post,
                Names = names,
                CMaps = cmaps,
                Locas = locas,
                Hmtx = hmetrics,
                Vmtx = vmetrics,
                Kern = kern
            };
        }

        private static readonly Random random = new();

        // Creates a unique subset prefix to be added to the font name when the font is embedded and subset
        private static string CreateSubsetPrefix()
        {
            char[] s = new char[7];
            lock (random)
            {
                for (int k = 0; k < 6; ++k)
                    s[k] = (char)(random.Next('A', 'Z' + 1));
            }
            s[6] = '+';
            return new string(s);
        }

        // Position 0 is the offset from the start of the file
        // and position 1 is the length of the table
        internal record OffsetItem
        {
            public uint Checksum { get; set; }

            public uint Offset { get; set; }

            public uint Length { get; set; }

            public byte[]? Data { get; set; }

            internal OffsetItem(uint checksum, uint offset, uint length)
            {
                Checksum = checksum;
                Offset = offset;
                Length = length;
            }
        }

        /// <summary>
        /// Table Directory
        /// </summary>
        internal Dictionary<string, OffsetItem> Tables { get; init; } = default!;

        /// <summary>
        /// Filled tables
        /// </summary>
        internal ushort FilledTables { get; init; } = default!;

        /// <summary>
        /// The content of table 'cff'
        /// CFF — Compact Font Format Table
        /// </summary>
        internal OffsetItem? Cff { get; init; }

        /// <summary>
        /// The content of table 'head'
        /// </summary>
        internal HeadTable Head { get; init; } = default!;

        /// <summary>
        /// The content of table 'hhea'
        /// </summary>
        internal HheaTable Hhea { get; init; } = default!;

        /// <summary>
        /// The content of table 'vhea'
        /// </summary>
        internal VheaTable? Vhea { get; init; }

        /// <summary>
        /// The content of table 'OS/2'
        /// </summary>
        internal MetricsTable Metrics { get; init; } = default!;

        /// <summary>
        /// The content of table 'post'
        /// </summary>
        internal PostTable Post { get; init; } = default!;

        /// <summary>
        /// The content of table 'name'
        /// </summary>
        internal FontName[] Names { get; init; } = default!;

        /// <summary>
        /// The content of table 'cmap'
        /// </summary>
        internal CMapFormat[] CMaps { get; init; } = default!;

        /// <summary>
        /// The content of table 'loca'
        /// </summary>
        internal int[] Locas { get; init; } = default!;

        /// <summary>
        /// The content of table 'hmtx'
        /// </summary>
        internal HMetrics[] Hmtx { get; init; } = default!;

        /// <summary>
        /// The content of table 'vmtx'
        /// </summary>
        internal VMetrics[]? Vmtx { get; init; }

        /// <summary>
        /// The content of table 'kern'
        /// </summary>
        internal Dictionary<int, float>? Kern { get; init; }

        /// <summary>
        /// Used glyphs, key = char int value, value = (glyph index (id), width)
        /// </summary>
        public SortedDictionary<int, (int Glyph, int Width)> UsedGlyphs { get; } = [];

        private PdfObject? streamRef;
        private PdfObject? toUnicodeRef;

        internal bool fontSpecific
        {
            get
            {
                return CMaps.Any(g => g.Platform == FontNamePlatform.Windows && g.EncodingID == 1);
            }
        }

        private string? uniqueIdentifier;

        internal string UniqueIdentifier
        {
            get
            {
                if (uniqueIdentifier == null)
                {
                    uniqueIdentifier = Names.FirstOrDefault(n => n.NameId == FontNameId.UniqueIdentifier)?.Name
                        ?? Names.First(n => n.NameId == FontNameId.FullName).Name;
                }
                return uniqueIdentifier;
            }
        }

        private string? postscriptName;

        internal string PostScriptName
        {
            get
            {
                if (postscriptName == null)
                {
                    postscriptName = Names.FirstOrDefault(n => n.NameId == FontNameId.PostScriptName)?.Name
                        ?? Names.First(n => n.NameId == FontNameId.FullName).Name;
                }

                return postscriptName;
            }
        }

        /// <summary>
        /// Transform font unit to local size
        /// https://docs.microsoft.com/en-us/typography/opentype/spec/ttch01
        /// 转换字体单位为本地大小
        /// </summary>
        /// <param name="fUnit">Font unit</param>
        /// <param name="size">Font size</param>
        /// <returns>Local size</returns>
        public float FUnitToLocal(int fUnit, float size)
        {
            return fUnit * size / 1000F;
        }

        internal PdfCIDFontDic GetCIDFontType(PdfObject fontDescriptor, string fontName)
        {
            string subtype;
            if (Cff is not null)
            {
                subtype = "CIDFontType0";
            }
            else
            {
                subtype = "CIDFontType2";
            }

            var dic = new PdfCIDFontDic(subtype, fontName, fontDescriptor);

            // Default width
            var dw = Hmtx.LastOrDefault()?.advanceWidth ?? 1000;
            dic.DW = dw;

            // W, without it, Latin character will the same width with unicode character
            // 如果没有W定义，字母'E'会和汉字一个宽度
            var w = new List<int>();

            var items = UsedGlyphs.Select(item => item.Value).OrderBy(item => item.Glyph);

            int? start = null;
            int? end = null;
            var width = 0;
            foreach (var (Glyph, Width) in items)
            {
                var gw = Width;

                if ((gw == dw || gw != width) && start != null)
                {
                    if (width != dw)
                    {
                        w.Add(start.Value);
                        w.Add(end.GetValueOrDefault(start.Value));
                        w.Add(width);
                    }

                    start = null;
                    end = null;
                }

                if (start == null)
                {
                    start = Glyph;
                    width = gw;
                }
                else if (gw == width)
                {
                    end = Glyph;
                }
            }

            if (start != null && width != dw)
            {
                w.Add(start.Value);
                w.Add(end.GetValueOrDefault(start.Value));
                w.Add(width);
            }

            if (w.Count > 0)
            {
                dic.W = [.. w];
            }

            return dic;
        }

        internal PdfFontDic GetFontBaseType(PdfObject descendant, string baseFont)
        {
            // Identity-H is a predefined CMap name
            var dic = new PdfFontDic(baseFont, "Identity-H")
            {
                DescendantFonts = [descendant]
            };

            return dic;
        }

        // 9.8.1 
        internal PdfFontDescriptor GetFontDescriptor(PdfObject fontStreamRef, string fontName)
        {
            var flags = 0;
            if (Post.isFixedPitch)
                flags |= 1;
            flags |= (fontSpecific ? 4 : 32);
            if ((Head.macStyle & 2) != 0)
                flags |= 64;
            if ((Head.macStyle & 1) != 0)
                flags |= 262144;

            var units = Head.unitsPerEm;

            // The thickness, measured horizontally, of the dominant vertical stems of glyphs in the font
            // https://stackoverflow.com/questions/35485179/stemv-value-of-the-truetype-font
            var stemV = 10 + 220 * (Metrics.usWeightClass - 50) / 900;

            var d = new PdfFontDescriptor(fontName)
            {
                Ascent = Metrics.sTypoAscender * 1000f / units,
                Descent = Metrics.sTypoDescender * 1000f / units,
                CapHeight = Metrics.sCapHeight * 1000f / units,
                StemV = stemV,
                FontBBox = new RectangleF(
                        Head.xMin * 1000f / units,
                        Head.yMin * 1000f / units,
                        (Head.xMax - Head.xMin) * 1000f / units,
                        (Head.yMax - Head.yMin) * 1000f / units
                    ),
                Flags = flags,
                ItalicAngle = Post.italicAngle
            };

            if (Cff is not null)
            {
                d.FontFile3 = fontStreamRef;
            }
            else
            {
                d.FontFile2 = fontStreamRef;
            }

            return d;
        }

        /// <summary>
        /// Get glyph height
        /// 获取字形高度
        /// </summary>
        /// <param name="glyph">Glyph id</param>
        /// <returns>Height</returns>
        public int GetGlyphHeight(int glyph)
        {
            if (Vmtx == null || Vmtx.Length == 0) return Metrics.sCapHeight;

            if (glyph >= Vmtx.Length)
                glyph = Vmtx.Length - 1;
            return Vmtx[glyph].advanceHeight;
        }

        /// <summary>
        /// Get glyph id
        /// 获取字形编号
        /// </summary>
        /// <param name="cid">Character id</param>
        /// <returns>Glyph id</returns>
        public int GetGlyphId(int cid)
        {
            var items = CMaps.OrderBy(g => g.Platform).OrderByDescending(g => g.Format);

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
        /// Get glyph width
        /// 获取字形宽度
        /// </summary>
        /// <param name="glyph">Glyph id</param>
        /// <returns>Width</returns>
        public int GetGlyphWidth(int glyph)
        {
            return GetHmtxItem(glyph).advanceWidth;
        }

        /// <summary>
        /// Get glyph width
        /// 获取字形宽度
        /// </summary>
        /// <param name="glyph">Glyph id</param>
        /// <returns>Width</returns>
        private HMetrics GetHmtxItem(int glyph)
        {
            if (glyph >= Hmtx.Length)
                glyph = Hmtx.Length - 1;

            return Hmtx[glyph];
        }

        /// <summary>
        /// Get line gap
        /// 获取线间距
        /// </summary>
        /// <param name="size">Font size</param>
        /// <returns>Result</returns>
        public float GetLineGap(float size)
        {
            if (Hhea.lineGap <= 0) return PdfFontUtils.GetLineGap(size);
            return Hhea.lineGap * size / Head.unitsPerEm;
        }

        /// <summary>
        /// Get subscript size and offset
        /// </summary>
        /// <param name="size">Font size</param>
        /// <returns>Result</returns>
        public PdfSizeAndOffset GetSubscript(float size)
        {
            if (Metrics.ySubscriptYSize <= 0) return PdfFontUtils.GetSubscript(size);

            var unitsPerEm = Head.unitsPerEm;
            return new PdfSizeAndOffset(Metrics.ySubscriptYSize * size / unitsPerEm, -Metrics.ySubscriptYOffset * size / unitsPerEm);
        }

        /// <summary>
        /// Get superscript size and offset
        /// </summary>
        /// <param name="size">Font size</param>
        /// <returns>Result</returns>
        public PdfSizeAndOffset GetSuperscript(float size)
        {
            if (Metrics.ySuperscriptYSize <= 0) return PdfFontUtils.GetSuperscript(size);

            var unitsPerEm = Head.unitsPerEm;
            return new PdfSizeAndOffset(Metrics.ySuperscriptYSize * size / unitsPerEm, Metrics.ySuperscriptYOffset * size / unitsPerEm);
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

        // Convert the character code to Unicode
        private async Task<PdfStreamDic> ToUnicodeAsync()
        {
            // Previous filter ".Where(g => g.Key >= 255)" is wrong
            // will causing copy / paste result unreadable
            var metrics = UsedGlyphs.Select(g => (g.Key, g.Value.Glyph)).ToArray();
            var len = metrics.Length;

            var buf = new StringBuilder(
                "/CIDInit /ProcSet findresource begin\n" +
                "12 dict begin\n" +
                "begincmap\n" +
                "/CIDSystemInfo\n" +
                "<< /Registry (Adobe)\n" +
                "/Ordering (UCS)\n" +
                "/Supplement 0\n" +
                ">> def\n" +
                "/CMapName /Adobe-Identity-UCS def\n" +
                "/CMapType 2 def\n" +
                "1 begincodespacerange\n" +
                "<0000> <FFFF>\n" +
                "endcodespacerange\n");

            var size = 0;
            for (var k = 0; k < len; ++k)
            {
                if (size == 0)
                {
                    if (k != 0)
                    {
                        buf.Append("endbfchar\n");
                    }
                    size = Math.Min(10000, len - k);
                    buf.Append(size).Append(" beginbfchar\n");
                }
                --size;
                var (Key, Glyph) = metrics[k];
                buf.Append(string.Format("<{0:X4}> <{1:X4}>\n", Glyph, Key));
            }

            buf.Append(
                "endbfchar\n" +
                "endcmap\n" +
                "CMapName currentdict /CMap defineresource pop\n" +
                "end end");

            var stream = PdfConstants.StreamManager.GetStream(Encoding.ASCII.GetBytes(buf.ToString()));
            var bytes = await stream.ToBytesAsync();

            return new PdfStreamDic(bytes);
        }

        private byte[] GetBytes(short n)
        {
            return [(byte)(n >> 8), (byte)n];
        }

        private byte[] GetBytes(ushort n)
        {
            return [(byte)(n >> 8), (byte)n];
        }

        private byte[] GetBytes(int n)
        {
            return [(byte)(n >> 24), (byte)(n >> 16), (byte)(n >> 8), (byte)n];
        }

        private byte[] GetBytes(uint n)
        {
            return [(byte)(n >> 24), (byte)(n >> 16), (byte)(n >> 8), (byte)n];
        }

        /// <summary>
        /// To subset
        /// 生成子集
        /// </summary>
        /// <returns>Result</returns>
        private async Task<PdfFontStream> ToSubsetAsync()
        {
            // Glyph blocks (Cmap for char to glyph id map)
            await using var newGlyfs = PdfConstants.StreamManager.GetStream();

            // The indexToLoc table stores the offsets to the locations of the glyphs in the font,
            // relative to the beginning of the glyphData table, indexed by glyph ID
            await using var newLocas = PdfConstants.StreamManager.GetStream();

            var glyfTable = Tables[GLYF];
            var glyfData = glyfTable.Data!;

            var glen = UsedGlyphs.Count;
            var glyhs = UsedGlyphs.Select(g => g.Value.Glyph).OrderBy(g => g).ToArray();

            var locaLen = Locas.Length;

            var listGlyf = 0;

            var glyfPtr = 0;

            for (var k = 0; k < locaLen - 1; k++)
            {
                // Updated offset
                if (Head.locaShortVersion)
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
            if (Head.locaShortVersion)
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

            // Writer
            using var aw = PdfConstants.StreamManager.GetStream();

            // Tag (4)
            aw.Write(GetBytes(0x00010000));

            // Tables (2)
            aw.Write(GetBytes(FilledTables));

            // 6 = 2 x 3
            var entrySelector = (ushort)entrySelectors[FilledTables];
            var searchRange = (ushort)((1 << entrySelector) * 16);
            var rangeShift = (ushort)((FilledTables - (1 << entrySelector)) * 16);
            aw.Write(GetBytes(entrySelector));
            aw.Write(GetBytes(searchRange));
            aw.Write(GetBytes(rangeShift));

            // Offset, 16 = Table dictionary length
            var offset = (uint)(16 * FilledTables + 12);

            foreach (var tableName in tableNames)
            {
                if (!Tables.TryGetValue(tableName, out var table))
                {
                    continue;
                }

                // Write name (4)
                aw.Write(PdfRandomAccessFileOrArray.Encoding1252.GetBytes(tableName));

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
            foreach (var tableName in tableNames)
            {
                if (!Tables.TryGetValue(tableName, out var table) || table.Data == null)
                {
                    continue;
                }

                for (var l = 0; l < lastAdjust; l++)
                {
                    aw.Write(PdfConstants.NullByte);
                }

                // Debug the subset structure
                /*
                if (aw.Length != table.Offset)
                {
                    Console.WriteLine($"Table offset {aw.Length} is not equal to current stream position {table.Offset}");
                }
                */

                await aw.WriteAsync(table.Data);

                var len = (uint)((table.Length + 3) & (~3));
                lastAdjust = len - table.Length;
            }

            // Back to start
            aw.Position = 0;


            // For debug
            // Try to load the subset and reparse again
            // await pdf.Fonts.LoadAsync("D:\\subset.ttf")
            //var fileStream = File.OpenWrite("D:\\subset.ttf");
            //await aw.CopyToAsync(fileStream);
            //aw.Position = 0;

            // Return stream dictionary
            return new PdfFontStream(await aw.ToBytesAsync())
            {
                Lengths = [(int)aw.Length]
            };
        }

        /// <summary>
        /// Write to stream
        /// 写入流
        /// </summary>
        /// <param name="writer">Stream writer</param>
        /// <param name="fontObj">Font obj reference</param>
        /// <param name="style">Font style</param>
        /// <returns>Task</returns>
        public async Task WriteAsync(IPdfWriter writer, PdfObject fontObj, PdfFontStyle? style)
        {
            // Font name
            var fontName = Cff == null ? Names.FirstOrDefault(n => n.NameId == FontNameId.PostScriptCID)?.Name : null;
            if (fontName == null)
            {
                fontName = CreateSubsetPrefix() + PostScriptName;
            }

            // Font subset
            if (streamRef == null)
            {
                var fontStream = await ToSubsetAsync();
                streamRef = await writer.WriteDicAsync(fontStream);
            }

            // ToUnicode
            if (toUnicodeRef == null)
            {
                var toUnicodeStream = await ToUnicodeAsync();
                toUnicodeRef = await writer.WriteDicAsync(toUnicodeStream);
            }

            // Font descriptor
            var desc = GetFontDescriptor(streamRef, fontName);
            if (style != null)
            {
                if (style.Value.HasFlag(PdfFontStyle.Bold))
                {
                    desc.FontWeight = 300;
                }
                if (style.Value.HasFlag(PdfFontStyle.Italic))
                {
                    desc.ItalicAngle = -30;
                }
            }
            var descRef = await writer.WriteDicAsync(desc);

            // CID font
            var cid = GetCIDFontType(descRef, fontName);
            var cidRef = await writer.WriteDicAsync(cid);

            // Base font
            var f = GetFontBaseType(cidRef, fontName);
            f.ToUnicode = toUnicodeRef;
            f.Obj = fontObj;
            await writer.WriteDicAsync(f);
        }
    }
}
