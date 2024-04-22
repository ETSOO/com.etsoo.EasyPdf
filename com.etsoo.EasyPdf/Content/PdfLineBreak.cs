using com.etsoo.EasyPdf.Support;
using System.Drawing;

namespace com.etsoo.EasyPdf.Content
{
    /// <summary>
    /// PDF line break
    /// PDF 换行
    /// </summary>
    public class PdfLineBreak : PdfChunk
    {
        /// <summary>
        /// Create new instance
        /// 创建新实例
        /// </summary>
        public static PdfLineBreak New => new();

        public PdfLineBreak() : base(PdfChunkType.LineBreak)
        {
        }

        public override Task<bool> WriteAsync(PdfWriter writer, RectangleF rect, PdfPoint point, PdfBlockLine line, Func<PdfBlockLine, PdfBlockLine?, Task> newLineAction)
        {
            point.X = rect.X;
            point.Y += line.Height;
            return Task.FromResult(false);
        }

        public override Task<bool> WriteInnerAsync(PdfWriter writer, PdfStyle style, RectangleF rect, PdfPoint point, PdfBlockLine line, Func<PdfBlockLine, PdfBlockLine?, Task> newLineAction)
        {
            return Task.FromResult(false);
        }
    }
}
