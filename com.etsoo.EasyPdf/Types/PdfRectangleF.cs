using System.Drawing;

namespace com.etsoo.EasyPdf.Types
{
    internal record PdfRectangleF(RectangleF Value) : IPdfType<RectangleF>
    {
        /// <summary>
        /// Constructor
        /// 构造函数
        /// </summary>
        /// <param name="llx">Lower-left x</param>
        /// <param name="lly">Lower-left y</param>
        /// <param name="urx">Uper-right x</param>
        /// <param name="ury">Uper-right y</param>
        public PdfRectangleF(float llx, float lly, float urx, float ury) : this(new RectangleF(llx, lly, urx - llx, ury - lly))
        {
            // MediaBox: [0 0 612 792]
        }

        public bool KeyEquals(string item)
        {
            return false;
        }

        public async Task WriteToAsync(Stream stream)
        {
            var arr = new PdfArray(new[] {
                new PdfReal((decimal)Value.Left),
                new PdfReal((decimal)Value.Top),
                new PdfReal((decimal)Value.Right),
                new PdfReal((decimal)Value.Bottom)
            });
            await arr.WriteToAsync(stream);
        }
    }
}
