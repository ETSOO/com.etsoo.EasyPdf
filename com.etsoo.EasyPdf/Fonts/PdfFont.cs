using com.etsoo.EasyPdf.Content;
using com.etsoo.EasyPdf.Types;

namespace com.etsoo.EasyPdf.Fonts
{
    /// <summary>
    /// PDF font
    /// PDF 字体
    /// </summary>
    internal class PdfFont : IPdfFont
    {
        /// <summary>
        /// Base font
        /// 基础字体
        /// </summary>
        public IPdfBaseFont BaseFont { get; }

        /// <summary>
        /// Is match the style
        /// 是否匹配样式
        /// </summary>
        public bool IsMatch { get; }

        /// <summary>
        /// Size
        /// 字体大小
        /// </summary>
        public float Size { get; }

        /// <summary>
        /// Style
        /// 样式
        /// </summary>
        public PdfFontStyle Style { get; }

        /// <summary>
        /// Reference name
        /// 引用名称
        /// </summary>
        public string RefName { get; }

        /// <summary>
        /// Object reference
        /// 对象引用
        /// </summary>
        public PdfObject? ObjRef { get; set; }

        /// <summary>
        /// Line gap
        /// 行间距
        /// </summary>
        public float LineGap => BaseFont.GetLineGap(Size);

        /// <summary>
        /// Line height
        /// 行高
        /// </summary>
        public float LineHeight => Size + LineGap;

        /// <summary>
        /// Subscript size and offset
        /// 下标大小和偏移量
        /// </summary>
        /// <param name="size">Font size</param>
        /// <returns>Result</returns>
        public PdfSizeAndOffset Subscript => BaseFont.GetSubscript(Size);

        /// <summary>
        /// Superscript size and offset
        /// 上标大小和偏移量
        /// </summary>
        /// <param name="size">Font size</param>
        /// <returns>Result</returns>
        public PdfSizeAndOffset Superscript => BaseFont.GetSuperscript(Size);

        // Character to Glyph Index Mapping Table for the font
        private readonly Dictionary<int, (int GlyphId, float Width, int FWidth)> cmap = [];

        /// <summary>
        /// Constructor
        /// 构造函数
        /// </summary>
        /// <param name="baseFont">Base font</param>
        /// <param name="size">Font size</param>
        /// <param name="style">Font style</param>
        /// <param name="refName">Ref name</param>
        public PdfFont(IPdfBaseFont baseFont, bool isMatch, float size, PdfFontStyle style, string refName)
        {
            BaseFont = baseFont;
            IsMatch = isMatch;
            Size = size;
            Style = style;
            RefName = refName;
        }

        /// <summary>
        /// Write the font
        /// 输出字体
        /// </summary>
        /// <param name="writer">Writer</param>
        public async Task WriteFontAsync(IPdfWriter writer)
        {
            if (ObjRef == null) return;

            await BaseFont.WriteAsync(writer, ObjRef, IsMatch ? null : Style);
        }

        /// <summary>
        /// Precalculate chunk
        /// 预计算内容块
        /// </summary>
        /// <param name="chunk">Chunk</param>
        /// <returns>Result</returns>
        public (char, float)[] Precalculate(PdfTextChunk chunk)
        {
            // Truetype, Identity encoding
            // Step 1: char code (cid) => glyph index
            // Step 2: font subset, glyph index to glyph to display
            // Step 3: ToUnicode reverse glyph index to unicode when read text

            var length = chunk.Content.Length;
            var glyfs = new (char Item, float Width)[length];
            for (var c = 0; c < length; c++)
            {
                var one = (int)chunk.Content.Span[c];
                if (!cmap.TryGetValue(one, out var citem))
                {
                    var glyphId = BaseFont.GetGlyphId(one);
                    var gwidth = BaseFont.GetGlyphWidth(glyphId);

                    var width = BaseFont.FUnitToLocal(gwidth, Size);

                    // Base font cache
                    if (!BaseFont.UsedGlyphs.ContainsKey(one)) BaseFont.UsedGlyphs.Add(one, (glyphId, gwidth));

                    // Font cache
                    cmap[one] = (glyphId, width, gwidth);

                    glyfs[c] = ((char)glyphId, width);
                }
                else
                {
                    glyfs[c] = ((char)citem.GlyphId, citem.Width);
                }
            }
            return glyfs;
        }

        /// <summary>
        /// Write chunk
        /// 写内容块
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="input">Input content</param>
        /// <returns>Task</returns>
        public async Task WriteAsync(Stream stream, IEnumerable<char> input)
        {
            var content = new PdfBinaryString(input);
            await content.WriteToAsync(stream, false);
        }
    }
}
