using AngleSharp.Dom;
using com.etsoo.EasyPdf.Content;

namespace com.etsoo.EasyPdf.Html
{
    internal class HtmlHR : HtmlBlock<PdfHR>
    {
        public HtmlHR(IElement element) : base(element)
        {

        }

        public override async Task<PdfBlock> ParseAsync(CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            return new PdfHR();
        }
    }
}
