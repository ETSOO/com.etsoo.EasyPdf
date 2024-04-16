using com.etsoo.EasyPdf.Objects;
using System.Drawing;

namespace com.etsoo.EasyPdf.Content
{
    public class PdfHR : PdfBlock
    {
        protected override async ValueTask WriteInnerAsync(IPdfPage page, PdfWriter writer, PdfStyle style, RectangleF rect)
        {
            await Task.CompletedTask;
        }
    }
}
