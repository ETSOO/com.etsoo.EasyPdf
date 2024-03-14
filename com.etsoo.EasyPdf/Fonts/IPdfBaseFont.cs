using com.etsoo.EasyPdf.Content;
using com.etsoo.EasyPdf.Types;

namespace com.etsoo.EasyPdf.Fonts
{
    /// <summary>
    /// PDF size and offset
    /// PDF 字体大小和偏移量
    /// </summary>
    /// <param name="Size">Size</param>
    /// <param name="Offset">Offset</param>
    public record PdfSizeAndOffset(float Size, float Offset);

    internal interface IPdfBaseFont
    {
        /// <summary>
        /// Used glyphs
        /// </summary>
        SortedDictionary<int, (int Glyph, int Width)> UsedGlyphs { get; }

        /// <summary>
        /// Transform font unit to local size
        /// https://docs.microsoft.com/en-us/typography/opentype/spec/ttch01
        /// 转换字体单位为本地大小
        /// </summary>
        /// <param name="fUnit">Font unit</param>
        /// <param name="size">Font size</param>
        /// <returns>Local size</returns>
        float FUnitToLocal(int fUnit, float size);

        /// <summary>
        /// Get glyph id
        /// 获取字形编号
        /// </summary>
        /// <param name="cid">Character id</param>
        /// <returns>Glyph id</returns>
        int GetGlyphId(int cid);

        /// <summary>
        /// Get glyph width
        /// 获取字形宽度
        /// </summary>
        /// <param name="glyph">Glyph id</param>
        /// <returns>Width</returns>
        int GetGlyphWidth(int glyph);

        /// <summary>
        /// Get line gap
        /// 获取线间距
        /// </summary>
        /// <param name="size">Font size</param>
        /// <returns>Result</returns>
        float GetLineGap(float size);

        /// <summary>
        /// Get subscript size and offset
        /// </summary>
        /// <param name="size">Font size</param>
        /// <returns>Result</returns>
        PdfSizeAndOffset GetSubscript(float size);

        /// <summary>
        /// Get superscript size and offset
        /// </summary>
        /// <param name="size">Font size</param>
        /// <returns>Result</returns>
        PdfSizeAndOffset GetSuperscript(float size);

        /// <summary>
        /// Write to stream
        /// 写入流
        /// </summary>
        /// <param name="writer">Stream writer</param>
        /// <param name="objRef">Font obj reference</param>
        /// <param name="style">Font style</param>
        /// <returns>Task</returns>
        Task WriteAsync(IPdfWriter writer, PdfObject objRef, PdfFontStyle? style);
    }
}
