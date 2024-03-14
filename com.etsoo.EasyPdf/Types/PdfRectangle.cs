using System.Drawing;

namespace com.etsoo.EasyPdf.Types
{
    internal record PdfRectangle(Rectangle Value) : IPdfType<Rectangle>
    {
        /// <summary>
        /// Constructor
        /// 构造函数
        /// </summary>
        /// <param name="llx">Lower-left x</param>
        /// <param name="lly">Lower-left y</param>
        /// <param name="urx">Uper-right x</param>
        /// <param name="ury">Uper-right y</param>
        public PdfRectangle(int llx, int lly, int urx, int ury) : this(new Rectangle(llx, lly, urx - llx, ury - lly))
        {
            // MediaBox: [0 0 612 792]
        }

        public bool KeyEquals(string item)
        {
            return false;
        }

        public async Task WriteToAsync(Stream stream)
        {
            var arr = new PdfArray(new[] { new PdfInt(Value.Left), new PdfInt(Value.Top), new PdfInt(Value.Right), new PdfInt(Value.Bottom) });
            await arr.WriteToAsync(stream);
        }
    }
}
