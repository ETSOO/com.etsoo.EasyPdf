using com.etsoo.EasyPdf.Objects;
using com.etsoo.EasyPdf.Support;
using com.etsoo.EasyPdf.Types;
using System.Drawing;

namespace com.etsoo.EasyPdf.Content
{
    /// <summary>
    /// PDF link
    /// PDF 链接
    /// </summary>
    public class PdfLink : PdfTextChunk
    {
        private static PdfColor DefaultColor => new(0, 0, 238);

        private readonly PDFURIAction action;
        private readonly string linkTitle;

        private PdfObject? actionRef;

        public PdfLink(ReadOnlySpan<char> content, Uri uri, string? title = null)
            : base(content, PdfChunkType.Link)
        {
            action = new PDFURIAction(uri.ToString());
            var url = $"{uri.Scheme}://{uri.Host}";
            linkTitle = string.IsNullOrEmpty(title) ? url : $"{title}\n{url}";
        }

        public override async Task NewLineActionAsync(PdfBlockLine line, int chunkIndex, IPdfPage page, PdfWriter writer, float width, PdfStyle style)
        {
            var chunk = line.Chunks[chunkIndex];
            var cwidth = line.Width - line.Chunks.Take(chunkIndex).Sum(c => c.Widths.Sum());
            var point = page.CalculatePoint(chunk.StartPoint);

            // Back to PDF coordinate
            // Adjust spaces
            var rect = new RectangleF(point.X - 1, point.Y - line.Height - chunk.Font.LineGap, cwidth + 2, chunk.Font.LineHeight);

            var link = new PdfLinkAnnotation(actionRef!, rect)
            {
                Contents = linkTitle
            };

            await writer.WriteLinkAsync(link);
        }

        public override async Task<bool> WriteAsync(PdfWriter writer, RectangleF rect, PdfPoint point, PdfBlockLine line, Func<PdfBlockLine, PdfBlockLine?, Task> newLineAction)
        {
            // Default styles
            Style.Color ??= DefaultColor;
            Style.TextDecoration ??= new PdfTextDecoration(PdfLineKind.Underline, Style.Color.GetValueOrDefault());

            // Action
            actionRef = await writer.WriteDicAsync(action);

            return await base.WriteAsync(writer, rect, point, line, newLineAction);
        }
    }
}
