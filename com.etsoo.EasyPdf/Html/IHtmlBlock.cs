using com.etsoo.EasyPdf.Content;

namespace com.etsoo.EasyPdf.Html
{
    internal interface IHtmlBlock
    {
        Task<PdfBlock> ParseAsync(CancellationToken cancellationToken);
    }
}
