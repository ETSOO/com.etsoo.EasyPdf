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

        /// <summary>
        /// Current page
        /// 当前页面
        /// </summary>
        public IPdfPage? CurrentPage => currentPage;

        private PdfObject? metadataObj;
        private IPdfFont? currentFont;
        private float? currentLineHeight;

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
        /// <returns>Page</returns>
        public async Task<IPdfPage> NewPageAsync(Action<IPdfPage>? setup = null)
        {
            if (currentPage is null)
            {
                // First page
                // Document information
                metadataObj = await WriteDicAsync(Document.Metadata);
            }
            else
            {
                // Write the current (very soon as previous) page
                await WritePageAsync(currentPage);
            }

            // Reset current font, each page has its own declaration
            currentFont = null;

            // Create a new page
            var data = currentPage?.Data ?? pageTree.PageData;
            var pageData = data with { };
            var style = currentPage?.Style ?? new PdfStyle(Document.Style);
            currentPage = new PdfPage(CreateObj(), pageTree, pageData, style);

            // Setup
            setup?.Invoke(currentPage);

            // Prepare for rendering
            await currentPage.PrepareAsync(this);

            return currentPage;
        }

        /// <summary>
        /// Write block element
        /// 输出块元素
        /// </summary>
        /// <param name="b">Block element</param>
        /// <returns>Task</returns>
        public async Task WriteAsync(PdfBlock b)
        {
            await currentPage!.WriteAsync(b, this);
        }

        /// <summary>
        /// Create font
        /// 创建字体
        /// </summary>
        /// <param name="familyName">Family name</param>
        /// <param name="size">Size in pt</param>
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
        /// <param name="operators">Operator bytes</param>
        /// <param name="style">Current style</param>
        /// <param name="required">Font reference is required</param>
        /// <returns>Current font and changed or not</returns>
        public (IPdfFont font, bool fontChanged) WriteFont(List<byte> operators, PdfStyle style, bool required = false)
        {
            var familyName = style.Font;
            if (string.IsNullOrEmpty(familyName))
            {
                return (currentFont!, false);
            }

            // Size is px, the to pt
            var size = style.FontSize.GetValueOrDefault(16).PxToPt();
            var fontStyle = style.FontStyle ?? PdfFontStyle.Regular;

            var font = CreateFont(familyName, size, fontStyle);

            // Avoid duplicate font operators
            var diffRef = currentFont?.RefName != font.RefName;
            var diffSize = currentFont?.Size != font.Size;
            if (required || diffRef || diffSize)
            {
                operators.AddRange(PdfOperator.Tf(font.RefName, size));
            }

            var fontLineHeight = font.LineHeight;
            var lineHeight = style.GetLineHeight(fontLineHeight);
            var diffHeight = currentLineHeight != lineHeight;
            if (required || diffRef || diffHeight)
            {
                operators.AddRange(PdfOperator.TL(lineHeight));
                currentLineHeight = lineHeight;
            }

            var diffFont = diffRef || diffSize || diffHeight;
            if (diffFont)
            {
                currentFont = font;
            }

            if (diffRef)
            {
                // Add to current page
                if (currentPage != null && font.ObjRef != null)
                    currentPage.Resources.Font[font.RefName] = font.ObjRef.AsRef();
            }

            // Even if the font is the same, the style (bold / italic) may be different
            // So, return font instead of currentFont
            return (font, diffFont);
        }

        /// <summary>
        /// Write font
        /// 输出字体
        /// </summary>
        /// <param name="stream">Stream to write</param>
        /// <param name="style">Current style</param>
        /// <param name="required">Font reference is required</param>
        /// <returns>Current font</returns>
        public async ValueTask<IPdfFont> WriteFontAsync(Stream stream, PdfStyle style, bool required = false)
        {
            var operators = new List<byte>();
            var (font, _) = WriteFont(operators, style, required);
            await stream.WriteAsync(operators.ToArray());
            return font;
        }

        private async Task WritePageAsync(PdfPage page)
        {
            // Finish writing
            await page.WriteEndAsync(this);

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
                currentPage = null;
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
