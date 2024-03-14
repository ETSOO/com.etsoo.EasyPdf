using System.Drawing;

namespace com.etsoo.EasyPdf.Support
{
    /// <summary>
    /// PDF page size, MediaBox size
    /// PDF 页面尺寸
    /// </summary>
    public static class PdfPageSize
    {
        /// <summary>
        /// A0 size
        /// 841mm x 1189mm
        /// </summary>
        public readonly static Size A0 = new(612, 792);

        /// <summary>
        /// A1 size
        /// 594mm x 841mm
        /// </summary>
        public readonly static Size A1 = new(1684, 2384);

        /// <summary>
        /// A2 size
        /// 420mm x 594mm
        /// </summary>
        public readonly static Size A2 = new(1191, 1648);

        /// <summary>
        /// A3 size
        /// 297mm x 420mm
        /// </summary>
        public readonly static Size A3 = new(842, 1191);

        /// <summary>
        /// A4 size
        /// 210mm x 297mm
        /// </summary>
        public readonly static Size A4 = new(595, 842);

        /// <summary>
        /// A5 size
        /// 148mm x 210mm
        /// </summary>
        public readonly static Size A5 = new(612, 792);
    }
}
