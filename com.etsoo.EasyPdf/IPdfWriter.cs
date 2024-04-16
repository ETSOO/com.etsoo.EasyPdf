using com.etsoo.EasyPdf.Content;
using com.etsoo.EasyPdf.Objects;

namespace com.etsoo.EasyPdf
{
    /// <summary>
    /// PDF content writer interface
    /// PDF 内容编写器接口
    /// </summary>
    public interface IPdfWriter : IAsyncDisposable
    {
        /// <summary>
        /// Current page
        /// 当前页面
        /// </summary>
        IPdfPage CurrentPage { get; }

        /// <summary>
        /// Start a new page
        /// 开始一个新页面
        /// </summary>
        /// <param name="setup">Page setup action</param>
        /// <returns>Page</returns>
        Task<IPdfPage> NewPageAsync(Action<IPdfPage>? setup = null);

        /// <summary>
        /// Write block element
        /// 输出块元素
        /// </summary>
        /// <param name="b">Block element</param>
        /// <returns>Task</returns>
        Task WriteAsync(PdfBlock b);
    }
}
