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
        private IPdfFont? currentFont;

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

            // Reset current font, each page has its own declaration
            currentFont = null;

            // Create a new page
            var pageData = pageTree.PageData with { };
            currentPage = new PdfPage(CreateObj(), pageTree, pageData, new PdfStyle(Document.Style));

            // Setup
            setup?.Invoke(currentPage);

            // Prepare for rendering
            await currentPage.PrepareAsync(this);
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
        /// <param name="size">Size in pt (not px)</param>
        /// <param name="style">Style</param>
        /// <returns>Font</returns>
        public IPdfFont CreateFont(string familyName, float size, PdfFontStyle style = PdfFontStyle.Regular)
        {
            var font = Document.Fonts.CreateFont(familyName, size, style);

            if (font.ObjRef == null)
            {
                // First time creation
                font.ObjRef = CreateObj();
            }

            return font;
        }

        /// <summary>
        /// Write font
        /// 输出字体
        /// </summary>
        /// <param name="stream">Stream to write</param>
        /// <param name="style">Current style</param>
        /// <returns>Current font</returns>
        public async ValueTask<IPdfFont> WriteFontAsync(Stream stream, PdfStyle style)
        {
            var familyName = style.Font;
            if (string.IsNullOrEmpty(familyName))
            {
                return currentFont!;
            }

            var size = style.FontSize.GetValueOrDefault(16).PxToPt();
            var fontStyle = style.FontStyle ?? PdfFontStyle.Regular;

            var font = CreateFont(familyName, size, fontStyle);

            // Avoid duplicate font operators
            if (currentFont?.RefName != font.RefName)
            {
                await stream.WriteAsync(PdfOperator.Tf(font.RefName, size));
                await stream.WriteAsync(PdfOperator.TL(size + font.LineGap));

                currentFont = font;

                // Add to current page
                if (currentPage != null && font.ObjRef != null)
                    currentPage.Resources.Font[font.RefName] = font.ObjRef.AsRef();
            }

            // Even if the font is the same, the style (bold / italic) may be different
            // So, return font instead of currentFont
            return font;
        }

        private async Task WritePageAsync(PdfPage page)
        {
            await using (page.Stream)
            {
                // Finish writing
                await page.WriteEndAsync();

                // Pdf stream
                var pageStream = new PdfStreamDic(page.Stream);
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
            var catalog = new PdfCatalog(pageTree.Obj.AsRef())
            {
                Lang = Document.Metadata.Culture?.TwoLetterISOLanguageName,
                URI = new PdfLinkBaseDic(Document.BaseUri)
            };
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
