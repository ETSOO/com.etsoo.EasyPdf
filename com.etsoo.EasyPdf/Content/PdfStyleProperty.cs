namespace com.etsoo.EasyPdf.Content
{
    /// <summary>
    /// PDF line style
    /// PDF 线条样式
    /// </summary>
    public enum PdfLineStyle
    {
        Solid,
        Dotted
    }

    /// <summary>
    /// PDF line kind
    /// PDf 线条类型
    /// </summary>
    public enum PdfLineKind
    {
        Underline,
        LineThrough
    }

    /// <summary>
    /// PDF text decoration
    /// PDF 文字修饰
    /// </summary>
    public record PdfTextDecoration
    {
        /// <summary>
        /// Kind
        /// 类型
        /// </summary>
        public PdfLineKind Kind { get; set; }

        /// <summary>
        /// Line style
        /// 线条样式
        /// </summary>
        public PdfLineStyle Style { get; set; }

        /// <summary>
        /// Color
        /// 颜色
        /// </summary>
        public PdfColor? Color { get; set; }

        /// <summary>
        /// Thickness
        /// 厚度
        /// </summary>
        public ushort Thickness { get; set; }
    }

    /// <summary>
    /// The font styles
    /// 字体样式
    /// </summary>
    [Flags]
    public enum PdfFontStyle
    {
        /// <summary>
        /// Regular
        /// </summary>
        Regular = 0,

        /// <summary>
        /// Bold
        /// </summary>
        Bold = 1,

        /// <summary>
        /// Italic
        /// </summary>
        Italic = 2,

        /// <summary>
        /// Bold and Italic
        /// </summary>
        BoldItalic = 3
    }

    /// <summary>
    /// PDF style space definition in pt (like padding and margin)
    /// PDF 样式空间定义
    /// </summary>
    /// <param name="Top">Top space</param>
    /// <param name="Right">Right space</param>
    /// <param name="Bottom">Bottom space</param>
    /// <param name="Left">Left space</param>
    public record PdfStyleSpace(int Top, int Right, int Bottom, int Left)
    {
        public PdfStyleSpace(int space) : this(space, space, space, space)
        {
        }

        public PdfStyleSpace(int vertical, int horizontal) : this(vertical, horizontal, vertical, horizontal)
        {
        }
    }

    /// <summary>
    /// PDF text style
    /// PDf 文字样式
    /// </summary>
    public enum PdfTextStyle
    {
        Normal,

        /// <summary>
        /// Superscript
        /// 上标
        /// </summary>
        SuperScript,

        /// <summary>
        /// Subscript
        /// 下标
        /// </summary>
        SubScript
    }
}
