using AngleSharp.Dom;
using com.etsoo.EasyPdf.Content;

namespace com.etsoo.EasyPdf.Html
{
    internal abstract class HtmlBlock<T> : IHtmlBlock where T : PdfBlock
    {
        protected IElement Element { get; }
        protected readonly string TagName;

        public HtmlBlock(IElement elment)
        {
            Element = elment;
            TagName = elment.TagName;
        }

        public abstract Task<PdfBlock> ParseAsync(CancellationToken cancellationToken);
    }
}
