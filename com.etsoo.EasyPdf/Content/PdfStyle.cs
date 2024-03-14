namespace com.etsoo.EasyPdf.Content
{
    /// <summary>
    /// PDF style
    /// PDF 样式
    /// </summary>
    public class PdfStyle
    {
        /// <summary>
        /// Parent style
        /// 父样式
        /// </summary>
        public PdfStyle? Parent { get; init; }

        /// <summary>
        /// Color
        /// 颜色
        /// </summary>
        public PdfColor? Color { get; set; }

        /// <summary>
        /// Font
        /// 字体
        /// </summary>
        public string? Font { get; set; }

        /// <summary>
        /// Font size
        /// 字体大小
        /// </summary>
        public float? FontSize { get; set; }

        /// <summary>
        /// Font style
        /// 字体样式
        /// </summary>
        public PdfFontStyle? FontStyle { get; set; }

        /// <summary>
        /// Letter spacing
        /// 字母间距
        /// </summary>
        public float? LetterSpacing { get; set; }

        /// <summary>
        /// Margin
        /// 外延距离
        /// </summary>
        public PdfStyleSpace? Margin { get; set; }

        /// <summary>
        /// Padding
        /// 填充距离
        /// </summary>
        public PdfStyleSpace? Padding { get; set; }

        /// <summary>
        /// Text decoration
        /// 文字修饰
        /// </summary>
        public PdfTextDecoration? TextDecoration { get; set; }

        /// <summary>
        /// Text style
        /// 文本样式
        /// </summary>
        public PdfTextStyle? TextStyle { get; set; }

        /// <summary>
        /// Word spacing
        /// 字间距
        /// </summary>
        public float? WordSpacing { get; set; }

        /// <summary>
        /// Constructor
        /// 构造函数
        /// </summary>
        /// <param name="parent">Parent style</param>
        public PdfStyle(PdfStyle? parent = null)
        {
            Parent = parent;
        }
    }
}