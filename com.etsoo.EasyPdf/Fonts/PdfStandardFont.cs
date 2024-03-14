using com.etsoo.EasyPdf.Content;
using com.etsoo.EasyPdf.Types;

namespace com.etsoo.EasyPdf.Fonts
{
    internal class PdfStandardFont : IPdfFont
    {
        public const string Helvetica = "Helvetica";
        public const string Times = "Times New Roman";
        public const string Courier = "Courier New";

        // All standards Type1 fonts (Symbol and ZapfDingbats not included)
        public static readonly string[][] Fonts = [
            [Helvetica, "Helvetica", "Helvetica-Bold", "Helvetica-Oblique", "Helvetica-BoldOblique"],
            [Times, "Times Roman", "Times-Bold", "Times-Italic", "Times-BoldItalic"],
            [Courier, "Courier", "Courier-Bold", "Courier-Oblique", "Courier-BoldOblique"]
        ];

        /// <summary>
        /// Is match the style
        /// 是否匹配样式
        /// </summary>
        public bool IsMatch { get; } = true;

        /// <summary>
        /// Base font
        /// 基础字体
        /// </summary>
        public string BaseFont { get; }

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
        public float LineGap => PdfFontUtils.GetLineGap(Size);

        /// <summary>
        /// Subscript size and offset
        /// 下标大小和偏移量
        /// </summary>
        /// <param name="size">Font size</param>
        /// <returns>Result</returns>
        public PdfSizeAndOffset Subscript => PdfFontUtils.GetSubscript(Size);

        /// <summary>
        /// Superscript size and offset
        /// 上标大小和偏移量
        /// </summary>
        /// <param name="size">Font size</param>
        /// <returns>Result</returns>
        public PdfSizeAndOffset Superscript => PdfFontUtils.GetSuperscript(Size);

        /// <summary>
        /// Constructor
        /// 构造函数
        /// </summary>
        /// <param name="baseFont">Base font</param>
        /// <param name="size">Font size</param>
        /// <param name="style">Font style</param>
        /// <param name="refName">Ref name</param>
        public PdfStandardFont(string baseFont, float size, PdfFontStyle style, string refName)
        {
            BaseFont = baseFont;
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

            var dic = new PdfStandardFontDic(BaseFont)
            {
                Obj = ObjRef
            };
            await writer.WriteDicAsync(dic);
        }

        /// <summary>
        /// Write chunk
        /// 写内容块
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="chunk">Chunk</param>
        /// <returns>Task</returns>
        public async Task WriteAsync(Stream stream, PdfChunk chunk)
        {
            if (chunk.Content.Span.IsAllAscii())
            {
                var content = new PdfString(chunk.Content.ToString());
                await content.WriteToAsync(stream);
            }
            else
            {
                var content = new PdfBinaryString(chunk.Content.ToString());
                await content.WriteToAsync(stream, false);
            }

            if (chunk.NewLine)
            {
                await stream.WriteAsync(PdfOperator.SQ);
            }
            else
            {
                await stream.WriteAsync(PdfOperator.Tj);
            }
        }
    }
}
