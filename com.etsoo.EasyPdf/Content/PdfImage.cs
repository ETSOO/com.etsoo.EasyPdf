using com.etsoo.EasyPdf.Support;
using System.Drawing;

namespace com.etsoo.EasyPdf.Content
{
    /// <summary>
    /// PDF image
    /// PDF 图片
    /// </summary>
    public class PdfImage : PdfChunk
    {
        public PdfImage(string path) : base(PdfChunkType.Image)
        {

        }

        public override async Task<bool> WriteAsync(PdfWriter writer, RectangleF rect, PdfPoint point, PdfBlockLine line, Func<PdfBlockLine, PdfBlockLine?, Task> newLineAction)
        {
            return false;
        }
    }
}
