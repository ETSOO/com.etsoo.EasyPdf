using com.etsoo.EasyPdf.Content;
using com.etsoo.EasyPdf.Fonts;
using com.etsoo.EasyPdf.Objects;
using com.etsoo.EasyPdf.Types;

namespace com.etsoo.EasyPdf
{
    /// <summary>
    /// PDF content writer interface
    /// PDF 内容编写器接口
    /// </summary>
    public interface IPdfWriter : IAsyncDisposable
    {
        /// <summary>
        /// Document
        /// 文档对象
        /// </summary>
        IPdfDocument Document { get; }

        /// <summary>
        /// Create font
        /// 创建字体
        /// </summary>
        /// <param name="familyName">Family name</param>
        /// <param name="size">Size</param>
        /// <param name="style">Style</param>
        /// <returns>Font</returns>
        IPdfFont CreateFont(string familyName, float size, PdfFontStyle style = PdfFontStyle.Regular);

        /// <summary>
        /// Start a new page
        /// 开始一个新页面
        /// </summary>
        /// <param name="setup">Page setup action</param>
        /// <returns>Task</returns>
        Task NewPageAsync(Action<IPdfPage>? setup = null);

        /// <summary>
        /// Async add dictionary data object
        /// 异步添加字典数据对象
        /// </summary>
        /// <param name="dic">Dictionary data object</param>
        /// <returns>Task</returns>
        Task<PdfObject> WriteDicAsync(PdfObjectDic dic);
    }
}
