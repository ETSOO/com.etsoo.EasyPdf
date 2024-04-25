using com.etsoo.EasyPdf.Objects;
using com.etsoo.EasyPdf.Support;
using System.Drawing;

namespace com.etsoo.EasyPdf.Content
{
    /// <summary>
    /// PDF horizontal rule
    /// PDF 水平线
    /// </summary>
    public class PdfHR : PdfBlock
    {
        /// <summary>
        /// Color
        /// 颜色
        /// </summary>
        public PdfColor Color { get; set; } = PdfColor.Black;

        /// <summary>
        /// Line width
        /// 线条宽度
        /// </summary>
        public float Width { get; set; } = 1;

        protected override async ValueTask<bool> WriteInnerAsync(IPdfPage page, PdfWriter writer, PdfStyle style, RectangleF rect, PdfPoint point)
        {
            await NewLineActionAsync(page, writer, style, rect, point, CurrentLine, null);
            return true;
        }

        public override void SetParentStyle(PdfStyle parentStyle, float fontSize)
        {
            base.SetParentStyle(parentStyle, fontSize);

            // Default styles
            Style.Margin ??= new PdfStyleSpace(6, 0);

            if (Style.Border == null)
            {
                var border = new PdfStyleBorder(Color, Width);
                border.Left.Width = 0;
                border.Right.Width = 0;
                border.Top.Width = 0;

                Style.Border = border;
            }
        }
    }
}
