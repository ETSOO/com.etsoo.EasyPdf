using com.etsoo.EasyPdf.Content;
using com.etsoo.EasyPdf.Dto;
using com.etsoo.EasyPdf.Fonts;
using com.etsoo.EasyPdf.Objects;

namespace com.etsoo.EasyPdf
{
    /// <summary>
    /// PDF document interface
    /// PDF 文档接口
    /// </summary>
    public interface IPdfDocument
    {
        /// <summary>
        /// Version, 1.4+ required
        /// 版本，仅支持1.4及以上版本
        /// </summary>
        decimal Version { get; }

        /// <summary>
        /// Page data
        /// 页面数据
        /// </summary>
        PdfPageData PageData { get; }

        /// <summary>
        /// Document metadata
        /// 文档元数据
        /// </summary>
        PdfMetadata Metadata { get; }

        /// <summary>
        /// Fonts collection
        /// 字体集合
        /// </summary>
        PdfFontCollection Fonts { get; }

        /// <summary>
        /// Style
        /// 样式
        /// </summary>
        PdfStyle Style { get; }
    }
}
