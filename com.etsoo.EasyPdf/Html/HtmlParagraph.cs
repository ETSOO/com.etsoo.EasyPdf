using AngleSharp.Dom;
using com.etsoo.EasyPdf.Content;

namespace com.etsoo.EasyPdf.Html
{
    internal class HtmlParagraph : HtmlRichBlock<PdfParagraph>
    {
        public HtmlParagraph(IElement element) : base(element)
        {

        }

        public override async Task<PdfBlock> ParseAsync(CancellationToken cancellationToken)
        {
            var p = new PdfParagraph();
            await ParseChildrenAsync(p, cancellationToken);
            return p;
        }
    }
}
