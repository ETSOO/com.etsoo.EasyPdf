namespace com.etsoo.EasyPdf.Content
{
    /// <summary>
    /// PDF paragraph, behaves like HTML P
    /// PDF 段落
    /// </summary>
    public class PdfParagraph : PdfRichBlock
    {
        public PdfParagraph() : base()
        {
        }

        public override void SetParentStyle(PdfStyle parentStyle, float fontSize)
        {
            base.SetParentStyle(parentStyle, fontSize);

            // Default styles
            Style.Margin ??= new PdfStyleSpace(12, 0);
        }
    }
}