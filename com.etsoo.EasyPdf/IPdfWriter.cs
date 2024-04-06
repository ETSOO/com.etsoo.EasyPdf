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
        /// Current page
        /// 当前页面
        /// </summary>
        public IPdfPage? CurrentPage { get; }

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
        /// <param name="size">Size in pt (not px)</param>
        /// <param name="style">Style</param>
        /// <returns>Font</returns>
        IPdfFont CreateFont(string familyName, float size, PdfFontStyle style = PdfFontStyle.Regular);

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

        /// <summary>
        /// Async add dictionary data object
        /// 异步添加字典数据对象
        /// </summary>
        /// <param name="dic">Dictionary data object</param>
        /// <returns>Task</returns>
        Task<PdfObject> WriteDicAsync(PdfObjectDic dic);

        /// <summary>
        /// Write font
        /// 输出字体
        /// </summary>
        /// <param name="operators">Operator bytes</param>
        /// <param name="style">Current style</param>
        /// <param name="required">Font reference is required</param>
        /// <returns>Current font and changed or not</returns>
        (IPdfFont font, bool fontChanged) WriteFont(List<byte> operators, PdfStyle style, bool required = false);

        /// <summary>
        /// Write font
        /// 输出字体
        /// </summary>
        /// <param name="stream">Stream to write</param>
        /// <param name="style">Current style</param>
        /// <param name="required">Font reference is required</param>
        /// <returns>Current font</returns>
        ValueTask<IPdfFont> WriteFontAsync(Stream stream, PdfStyle style, bool required = false);
    }
}
