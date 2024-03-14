using com.etsoo.EasyPdf.Content;
using com.etsoo.EasyPdf.Dto;

namespace com.etsoo.EasyPdf.Objects
{
    /// <summary>
    /// PDF page object interface
    /// PDF 页面对象接口
    /// </summary>
    public interface IPdfPage
    {
        /// <summary>
        /// Page data
        /// 页面数据
        /// </summary>
        PdfPageData Data { get; }

        /// <summary>
        /// Page stream
        /// 页面流
        /// </summary>
        Stream Stream { get; }

        /// <summary>
        /// Style
        /// 样式
        /// </summary>
        PdfStyle Style { get; }
    }
}
