using com.etsoo.EasyPdf.Content;
using com.etsoo.EasyPdf.Types;

namespace com.etsoo.EasyPdf.Fonts
{
    public interface IPdfFont
    {
        /// <summary>
        /// Is match the style
        /// 是否匹配样式
        /// </summary>
        bool IsMatch { get; }

        /// <summary>
        /// Size
        /// 字体大小
        /// </summary>
        float Size { get; }

        /// <summary>
        /// Style
        /// 样式
        /// </summary>
        PdfFontStyle Style { get; }

        /// <summary>
        /// Reference name
        /// 引用名称
        /// </summary>
        string RefName { get; }

        /// <summary>
        /// Object reference
        /// 对象引用
        /// </summary>
        PdfObject? ObjRef { get; set; }

        /// <summary>
        /// Line gap
        /// 行间距
        /// </summary>
        float LineGap { get; }

        /// <summary>
        /// Line height
        /// 行高
        /// </summary>
        float LineHeight { get; }

        /// <summary>
        /// Subscript size and offset
        /// 下标大小和偏移量
        /// </summary>
        /// <param name="size">Font size</param>
        /// <returns>Result</returns>
        PdfSizeAndOffset Subscript { get; }

        /// <summary>
        /// Superscript size and offset
        /// 上标大小和偏移量
        /// </summary>
        /// <param name="size">Font size</param>
        /// <returns>Result</returns>
        PdfSizeAndOffset Superscript { get; }

        /// <summary>
        /// Local size to font unit
        /// 本地大小转换为字体单位
        /// </summary>
        /// <param name="size">Local size</param>
        /// <returns>Result</returns>
        float LocalSizeToFUnit(float size);

        /// <summary>
        /// Precalculate chunk
        /// 预计算内容块
        /// </summary>
        /// <param name="chunk">Chunk</param>
        /// <returns>Result</returns>
        (char, float)[] Precalculate(PdfTextChunk chunk);

        /// <summary>
        /// Write chunk
        /// 写内容块
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="input">Input content</param>
        /// <returns>Task</returns>
        Task WriteAsync(Stream stream, IEnumerable<char> input);

        /// <summary>
        /// Write the font
        /// 输出字体
        /// </summary>
        /// <param name="writer">Writer</param>
        Task WriteFontAsync(PdfWriter writer);
    }
}
