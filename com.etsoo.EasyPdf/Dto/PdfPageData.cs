using com.etsoo.EasyPdf.Support;
using System.Drawing;

namespace com.etsoo.EasyPdf.Dto
{
    /// <summary>
    /// PDF page data
    /// PDF 页面数据
    /// </summary>
    public record PdfPageData
    {
        private RectangleF? mediaBox;
        /// <summary>
        /// Define the boundaries of the physical medium on which the page shall be displayed or printed
        /// 定义页面显示或打印的物理介质的边界
        /// </summary>
        public RectangleF MediaBox
        {
            get
            {
                if (mediaBox.HasValue) return mediaBox.Value;
                else return new RectangleF(0, 0, PageSize.Width, PageSize.Height);
            }
            set
            {
                mediaBox = value;
                PageSize = value.Size;
            }
        }

        /// <summary>
        /// Page size
        /// 页面大小
        /// </summary>
        public SizeF PageSize { get; set; } = PdfPageSize.A4;

        /// <summary>
        /// The number of degrees by which the page shall be rotated clockwise when displayed or printed.
        /// The value shall be a multiple of 90, default is 0.
        /// 页面显示或打印时顺时针旋转的度数。
        /// </summary>
        public int? Rotate { get; set; }

        /// <summary>
        /// User unit, for simplicty, ignored
        /// 1/72 inch = 1 point, when PDFReader view resolutin is 75px / inch, the same size
        /// When it's 120px / inch (looks larger), 60px will be 75/76px (125%)
        /// A4 595pt => 793.33px => around 992px in 120px/inch screen
        /// 用户单位，为简单起见，忽略
        /// </summary>
        // public float UserUnit { get; set; } = 1.0F;
    }
}
