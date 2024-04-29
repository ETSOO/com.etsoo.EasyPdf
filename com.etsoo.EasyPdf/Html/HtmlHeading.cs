using AngleSharp.Dom;
using com.etsoo.EasyPdf.Content;

namespace com.etsoo.EasyPdf.Html
{
    internal class HtmlHeading : HtmlRichBlock<PdfHeading>
    {
        public HtmlHeading(IElement element) : base(element)
        {

        }

        public override async Task<PdfBlock> ParseAsync(CancellationToken cancellationToken)
        {
            var level = Enum.TryParse<PdfHeading.Level>(TagName, out var l) ? l : PdfHeading.Level.H1;

            var h = new PdfHeading(level);
            await ParseChildrenAsync(h, cancellationToken);

            return h;
        }
    }
}
