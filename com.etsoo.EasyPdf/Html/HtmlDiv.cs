using AngleSharp.Dom;
using com.etsoo.EasyPdf.Content;

namespace com.etsoo.EasyPdf.Html
{
    internal class HtmlDiv : HtmlRichBlock<PdfDiv>
    {
        public HtmlDiv(IElement element) : base(element)
        {

        }

        public override async Task<PdfBlock> ParseAsync(CancellationToken cancellationToken)
        {
            var div = new PdfDiv();
            await ParseChildrenAsync(div, cancellationToken);
            return div;
        }
    }
}
