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
        private Rectangle? mediaBox;
        /// <summary>
        /// Define the boundaries of the physical medium on which the page shall be displayed or printed
        /// 定义页面显示或打印的物理介质的边界
        /// </summary>
        public Rectangle MediaBox
        {
            get
            {
                if (mediaBox.HasValue) return mediaBox.Value;
                else return new Rectangle(0, 0, PageSize.Width, PageSize.Height);
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
        public Size PageSize { get; set; } = PdfPageSize.A4;

        /// <summary>
        /// The number of degrees by which the page shall be rotated clockwise when displayed or printed.
        /// The value shall be a multiple of 90, default is 0.
        /// 页面显示或打印时顺时针旋转的度数。
        /// </summary>
        public int? Rotate { get; set; }
    }
}
