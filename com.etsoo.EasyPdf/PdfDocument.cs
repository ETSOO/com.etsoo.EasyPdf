using com.etsoo.EasyPdf.Content;
using com.etsoo.EasyPdf.Fonts;
using com.etsoo.EasyPdf.Objects;
using com.etsoo.EasyPdf.Support;
using com.etsoo.EasyPdf.Types;

namespace com.etsoo.EasyPdf
{
    /// <summary>
    /// PDF document creation, based on PDF 1.7
    /// PDF 文档创建，基于 PDF 1.7
    /// A basic conforming PDF files shall be constructed of following four elements
    /// 1. A one-line header identifying that make up the document contained in the file.
    /// 2. A body containing the objects that make up the document contained in the file.
    /// 3. A cross-reference table containing information about the indirect objects in the file.
    /// 4. A trailer giving the location of the cross-reference table and of certain special objects within the body of the file.
    /// https://stackoverflow.com/questions/10287602/show-pdf-with-all-operators to decode PDF file
    /// qpdf --qdf etsoo2.pdf source2.pdf
    /// Example files included in the folder "Resources" of the project com.etsoo.EasyPdf.Tests.csproj
    /// </summary>
    public class PdfDocument : IPdfDocument, IDisposable, IAsyncDisposable
    {
        /// <summary>
        /// Debug mode
        /// 调试模式
        /// </summary>
        public static bool Debug { get; set; }

        private readonly Stream stream;
        private readonly bool keepOpen;
        private PdfWriter? writer;

        /// <summary>
        /// Version
        /// 版本
        /// </summary>
        public decimal Version { get; private set; } = PdfVersion.V17;

        /// <summary>
        /// Base URI
        /// 基础 URI
        /// </summary>
        public string? BaseUri { get; set; }

        /// <summary>
        /// Page data
        /// 页面数据
        /// </summary>
        public PdfPageData PageData { get; } = new()
        {
            PageSize = PdfPageSize.A4
        };

        /// <summary>
        /// Style
        /// 样式
        /// </summary>
        public PdfStyle Style { get; } = new()
        {
            Font = "Arial",
            // 16px = 12pt
            FontSize = 16,
            Padding = new PdfStyleSpace(60)
        };

        /// <summary>
        /// Document metadata
        /// 文档元数据
        /// </summary>
        public PdfMetadata Metadata { get; } = new();

        /// <summary>
        /// Fonts collection
        /// 字体集合
        /// </summary>
        public PdfFontCollection Fonts { get; } = new();

        /// <summary>
        /// Header
        /// 页眉
        /// </summary>
        public PdfHeaderFooter Header { get; } = new PdfHeaderFooter(true);

        /// <summary>
        /// Footer
        /// 页脚
        /// </summary>
        public PdfHeaderFooter Footer { get; } = new PdfHeaderFooter(false);

        /// <summary>
        /// Constructor
        /// 构造函数
        /// </summary>
        /// <param name="saveStream">Save stream</param>
        /// <param name="keepOpen">Keep the stream open after writing</param>
        public PdfDocument(Stream saveStream, bool keepOpen = false)
        {
            stream = saveStream;
            this.keepOpen = keepOpen;
        }

        /// <summary>
        /// Async get writer and start writing
        /// 异步获取写入器并开始写入
        /// </summary>
        /// <returns>PDF writer</returns>
        public async ValueTask<PdfWriter> GetWriterAsync()
        {
            if (writer == null)
            {
                // First line
                var version = new PdfVersion(Version);
                await version.WriteToAsync(stream);

                // Second line
                // If a PDF file contains binary data, as most do (see 7.2, Lexical Conventions"),
                // the header line shall be immediately followed by a comment line containing at least four binary characters—that is,
                // characters whose codes are 128 or greater. This ensures proper behaviour of file transfer applications that inspect data near the beginning of a file to determine whether to treat the file’s contents as text or as binary.
                var binarCheck = new PdfBinaryCheck();
                await binarCheck.WriteToAsync(stream);

                // Writer
                writer = new PdfWriter(this, stream);
            }

            return writer;
        }

        public void Dispose()
        {
            if (writer != null)
            {
                writer.DisposeAsync().ConfigureAwait(false).GetAwaiter().GetResult();
                writer = null;
            }

            if (!keepOpen)
            {
                stream.Dispose();
            }

            GC.SuppressFinalize(this);
        }

        public async ValueTask DisposeAsync()
        {
            if (writer != null)
            {
                await writer.DisposeAsync();
                writer = null;
            }

            if (!keepOpen)
            {
                await stream.DisposeAsync();
            }

            GC.SuppressFinalize(this);
        }
    }
}
