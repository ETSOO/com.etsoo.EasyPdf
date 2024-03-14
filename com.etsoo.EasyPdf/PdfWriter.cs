using com.etsoo.EasyPdf.Content;
using com.etsoo.EasyPdf.Fonts;
using com.etsoo.EasyPdf.Objects;
using com.etsoo.EasyPdf.Types;
using System.Text;

namespace com.etsoo.EasyPdf
{
    /// <summary>
    /// PDF content writer
    /// PDF 内容编写器
    /// </summary>
    public class PdfWriter : IPdfWriter
    {
        private readonly SortedDictionary<ushort, PdfReference> refs = new() { [0] = new PdfReference(0, 65535, true) };

        private readonly Stream stream;
        private readonly PdfPageTree pageTree;

        private bool disposed = false;

        private ushort objIndex = 0;
        private PdfPage? currentPage;
        private PdfObject? metadataObj;

        /// <summary>
        /// Document
        /// 文档对象
        /// </summary>
        public IPdfDocument Document { get; }

        /// <summary>
        /// Constructor
        /// 构造函数
        /// </summary>
        /// <param name="document">Document interface</param>
        /// <param name="saveStream">Save stream</param>
        public PdfWriter(IPdfDocument document, Stream saveStream)
        {
            Document = document;
            stream = saveStream;

            var obj = CreateObj();
            pageTree = new PdfPageTree(obj, document.PageData);
        }

        /// <summary>
        /// 1 0 obj
        /// Create reference obj
        /// 创建索引对象
        /// </summary>
        /// <returns>Result</returns>
        private PdfObject CreateObj()
        {
            // Next index
            objIndex++;

            // Reference obj
            return new PdfObject(objIndex, false);
        }

        /// <summary>
        /// Async add dictionary data object
        /// 异步添加字典数据对象
        /// </summary>
        /// <param name="dic">Dictionary data object</param>
        /// <returns>Task</returns>
        public async Task<PdfObject> WriteDicAsync(PdfObjectDic dic)
        {
            // Current position
            var pos = (uint)stream.Position;

            // When obj is null
            if (dic.Obj == null)
            {
                dic.Obj = CreateObj();
            }

            // Write to stream
            await dic.WriteToAsync(stream);

            // Add a reference
            refs.Add(dic.Obj.Value, new PdfReference(pos, 0));

            // Return
            return dic.Obj.AsRef();
        }

        /// <summary>
        /// Start a new page
        /// 开始一个新页面
        /// </summary>
        /// <param name="setup">Page setup action</param>
        /// <returns>Task</returns>
        public async Task NewPageAsync(Action<IPdfPage>? setup = null)
        {
            if (currentPage is null)
            {
                // First page
                // Document information
                metadataObj = await WriteDicAsync(Document.Metadata);
            }
            else
            {
                await WritePageAsync(currentPage);
            }

            // Create a new page
            currentPage = new PdfPage(CreateObj(), pageTree, pageTree.PageData, new PdfStyle(Document.Style));

            // Setup
            setup?.Invoke(currentPage);

            // Prepare for rendering
            await currentPage.PrepareAsync();
        }

        /// <summary>
        /// Add paragraph
        /// 添加段落
        /// </summary>
        /// <param name="p">Paragraph</param>
        /// <returns>Task</returns>
        public async Task AddAsync(PdfBlock p)
        {
            if (await currentPage!.WriteAsync(p, this))
            {
                // Auto create new page without setup action
                await NewPageAsync();
            }
        }

        /// <summary>
        /// Create font
        /// 创建字体
        /// </summary>
        /// <param name="familyName">Family name</param>
        /// <param name="size">Size</param>
        /// <param name="style">Style</param>
        /// <returns>Font</returns>
        public IPdfFont CreateFont(string familyName, float size, PdfFontStyle style = PdfFontStyle.Regular)
        {
            var font = Document.Fonts.CreateFont(familyName, size, style);

            if (font.ObjRef == null)
            {
                // First time creation
                font.ObjRef = CreateObj();

                // Add to current page
                if (currentPage != null)
                    currentPage.Resources.Font[font.RefName] = font.ObjRef.AsRef();
            }

            return font;
        }

        private async Task WritePageAsync(PdfPage page)
        {
            await using (page.Stream)
            {
                // Finish writing
                await page.WriteEndAsync();

                // Pdf stream
                var pageStream = new PdfStreamDic(page.Stream, null);
                page.Contents = await WriteDicAsync(pageStream);

                // Dispose
                await page.Stream.DisposeAsync();
            }

            // Write page
            var pageObj = await WriteDicAsync(page);

            // Add to kids
            pageTree.Kids.Add(pageObj);
        }

        private async Task WriteXref()
        {
            await stream.WriteAsync(PdfConstants.XrefBytes);
            stream.WriteByte(PdfConstants.LineFeedByte);

            await stream.WriteAsync(Encoding.ASCII.GetBytes($"0 {refs.Count}"));
            stream.WriteByte(PdfConstants.LineFeedByte);

            foreach (var r in refs)
            {
                await r.Value.WriteToAsync(stream);
            }
        }

        /// <summary>
        /// Async dispose
        /// 异步释放
        /// </summary>
        /// <returns>Task</returns>
        public async ValueTask DisposeAsync()
        {
            // Avoid multiple calls
            if (disposed) return;

            if (currentPage != null)
            {
                await WritePageAsync(currentPage);
            }

            // Write page tree
            await WriteDicAsync(pageTree);

            // catalog / root
            var catalog = new PdfCatalog(pageTree.Obj.AsRef()) { Lang = Document.Metadata.Culture?.TwoLetterISOLanguageName };
            var catalogObj = await WriteDicAsync(catalog);

            // All fonts
            await Document.Fonts.WriteAsyc(this);

            // startxref
            var startxref = (int)stream.Position;

            // xref
            await WriteXref();

            // trailer & startxref
            var trailer = new PdfTrailer(startxref, refs.Count, catalogObj)
            {
                Info = metadataObj
            };
            await trailer.WriteToAsync(stream);

            // End of File
            var eof = new PdfEOF();
            await eof.WriteToAsync(stream);

            // Update flag
            disposed = true;

            GC.SuppressFinalize(this);
        }
    }
}
