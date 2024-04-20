using com.etsoo.EasyPdf.Content;
using com.etsoo.EasyPdf.Fonts;
using com.etsoo.EasyPdf.Objects;
using com.etsoo.EasyPdf.Types;
using System.Diagnostics.CodeAnalysis;
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

        [AllowNull]
        private PdfPage currentPage;

        /// <summary>
        /// Current page
        /// 当前页面
        /// </summary>
        public IPdfPage CurrentPage => currentPage;

        private PdfObject? metadataObj;

        private IPdfFont? currentFont;

        /// <summary>
        /// Current font
        /// 当前字体
        /// </summary>
        internal IPdfFont? CurrentFont => currentFont;

        private float? currentLineHeight;

        /// <summary>
        /// Document
        /// 文档对象
        /// </summary>
        internal IPdfDocument Document { get; }

        /// <summary>
        /// Constructor
        /// 构造函数
        /// </summary>
        /// <param name="document">Document interface</param>
        /// <param name="saveStream">Save stream</param>
        internal PdfWriter(IPdfDocument document, Stream saveStream)
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
        public PdfObject CreateObj()
        {
            // Next index
            objIndex++;

            // Reference obj
            return new PdfObject(objIndex, false);
        }

        /// <summary>
        /// Create font
        /// 创建字体
        /// </summary>
        /// <param name="familyName">Family name</param>
        /// <param name="size">Size in pt</param>
        /// <param name="style">Style</param>
        /// <returns>Font</returns>
        internal IPdfFont CreateFont(string familyName, float size, PdfFontStyle style = PdfFontStyle.Regular)
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
        /// Define opacity
        /// 定义不透明度
        /// </summary>
        /// <param name="opacity">Opacity value</param>
        /// <param name="withWrite">With writing the bytes</param>
        /// <returns>Bytes</returns>
        internal async Task<byte[]> DefineOpacityAsync(float opacity, bool withWrite = false)
        {
            var dic = new PdfExtGState
            {
                CA = opacity,
                ca = opacity
            };

            var gsObj = await WriteDicAsync(dic);

            var gsStates = currentPage.Resources.ExtGState ?? throw new InvalidOperationException("No current page");
            var refName = $"GS{gsStates.Count}";
            gsStates[refName] = gsObj.AsRef();

            var bytes = PdfOperator.Zgs(refName);

            if (withWrite)
            {
                await currentPage.Stream.WriteAsync(bytes);
            }

            return bytes;
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
                Lang = Document.Metadata.Culture?.TwoLetterISOLanguageName
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
            await currentPage.WriteAsync(b, this);
        }

        /// <summary>
        /// Async add dictionary data object
        /// 异步添加字典数据对象
        /// </summary>
        /// <param name="dic">Dictionary data object</param>
        /// <returns>Task</returns>
        internal async Task<PdfObject> WriteDicAsync(PdfObjectDic dic)
        {
            // When obj is null
            if (dic.Obj == null)
            {
                dic.Obj = CreateObj();
            }

            // Write related streams, may change the stream position
            await dic.WriteRelatedStreams(this, stream);

            // Current position
            var pos = (uint)stream.Position;

            // Write to stream
            await dic.WriteToAsync(stream);

            // Add a reference
            refs.Add(dic.Obj.Value, new PdfReference(pos, 0));

            // Return
            return dic.Obj.AsRef();
        }

        /// <summary>
        /// Write font
        /// 输出字体
        /// </summary>
        /// <param name="font">Font</param>
        /// <returns>Bytes</returns>
        internal byte[] WriteFont(IPdfFont font)
        {
            // Add to current page
            if (font.ObjRef != null && !currentPage.Resources.Font.ContainsKey(font.RefName))
                currentPage.Resources.Font[font.RefName] = font.ObjRef.AsRef();

            // Return bytes
            return PdfOperator.Tf(font.RefName, font.Size);
        }

        /// <summary>
        /// Write font
        /// 输出字体
        /// </summary>
        /// <param name="operators">Operator bytes</param>
        /// <param name="style">Current style</param>
        /// <param name="required">Font reference is required</param>
        /// <returns>Current font and changed or not</returns>
        internal (IPdfFont font, bool fontChanged) WriteFont(List<byte[]> operators, PdfStyle style, bool required = false)
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
                operators.Add(PdfOperator.Tf(font.RefName, size));
            }

            var fontLineHeight = font.LineHeight;
            var lineHeight = style.GetLineHeight(fontLineHeight);
            var diffHeight = currentLineHeight != lineHeight;
            if (required || diffRef || diffHeight)
            {
                operators.Add(PdfOperator.TL(lineHeight));
                currentLineHeight = lineHeight;
            }

            var diffFont = diffRef || diffSize || diffHeight;
            if (diffFont)
            {
                currentFont = font;
            }

            if (diffRef && font.ObjRef != null)
            {
                // Add to current page
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
        internal async ValueTask<IPdfFont> WriteFontAsync(Stream stream, PdfStyle style, bool required = false)
        {
            var operators = new List<byte[]>();
            var (font, _) = WriteFont(operators, style, required);

            foreach (var op in operators)
            {
                await stream.WriteAsync(op);
            }

            return font;
        }

        /// <summary>
        /// Write link
        /// 输出链接
        /// </summary>
        /// <param name="link">Link</param>
        /// <returns>Task</returns>
        internal async Task WriteLinkAsync(PdfLinkAnnotation link)
        {
            var linkRef = await WriteDicAsync(link);
            currentPage.Annots.Add(linkRef.AsRef());
        }

        /// <summary>
        /// Write image
        /// 输出图片
        /// </summary>
        /// <param name="image">Image stream</param>
        /// <returns>Reference name</returns>
        internal string WriteImage(PdfImageStream image)
        {
            // Name
            var name = $"Im{currentPage.Resources.XObject.Count}";

            // Take the index for the object
            image.Obj = CreateObj();

            // Add to current page
            currentPage.Resources.XObject[name] = image;

            // Return the name
            return name;
        }

        private async Task WritePageAsync(PdfPage page)
        {
            // Finish writing
            await page.WriteEndAsync(this);

            // Write XObject
            // A good idea to write all streams first
            foreach (var x in page.Resources.XObject)
            {
                if (x.Value.Obj != null)
                {
                    var obj = await WriteDicAsync(x.Value);
                    page.Resources.XObjectRefs[x.Key] = obj;
                }
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
    }
}
