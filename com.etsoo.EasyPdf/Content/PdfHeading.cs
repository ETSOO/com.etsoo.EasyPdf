namespace com.etsoo.EasyPdf.Content
{
    /// <summary>
    /// PDF heading, behaves like HTML H1, H2, H3, H4, H5, H6
    /// PDF 标题
    /// </summary>
    public class PdfHeading : PdfRichBlock
    {
        /// <summary>
        /// Heading level
        /// 标题级别
        /// </summary>
        public enum Level
        {
            H1,
            H2,
            H3,
            H4,
            H5,
            H6
        }

        public Level HeadingLevel { get; set; }

        /// <summary>
        /// Constructor
        /// 构造函数
        /// </summary>
        /// <param name="level">级别</param>
        public PdfHeading(Level level) : base()
        {
            HeadingLevel = level;
        }

        public override void SetParentStyle(PdfStyle parentStyle, float fontSize)
        {
            base.SetParentStyle(parentStyle, fontSize);

            // Default styles
            Style.FontStyle ??= PdfFontStyle.Bold;
            Style.FontSize ??= HeadingLevel switch
            {
                Level.H1 => 24,
                Level.H2 => 18,
                Level.H3 => 14,
                Level.H5 => 10,
                Level.H6 => 8,
                _ => 12
            };
            Style.Margin ??= HeadingLevel switch
            {
                Level.H1 => new PdfStyleSpace(8, 0),
                Level.H2 => new PdfStyleSpace(10, 0),
                Level.H4 => new PdfStyleSpace(16, 0),
                Level.H5 => new PdfStyleSpace(20, 0),
                Level.H6 => new PdfStyleSpace(28, 0),
                _ => new PdfStyleSpace(12, 0)
            };
        }
    }
}
