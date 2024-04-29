using AngleSharp.Css;
using AngleSharp.Css.Dom;
using AngleSharp.Css.Values;
using AngleSharp.Dom;
using AngleSharp.Dom.Events;
using AngleSharp.Html.Dom;
using com.etsoo.EasyPdf.Content;
using com.etsoo.EasyPdf.Html;
using com.etsoo.EasyPdf.Support;
using com.etsoo.HtmlUtils;
using System.Drawing;
using System.Globalization;
using System.Runtime.InteropServices;

namespace com.etsoo.EasyPdf
{
    /// <summary>
    /// Html parser extension
    /// Html 解析器扩展
    /// </summary>
    public static class PdfDocumentHtmlParser
    {
        /// <summary>
        /// Requested resources
        /// 请求的资源
        /// </summary>
        public static readonly Dictionary<string, ReadOnlyMemory<byte>> Resources = [];

        private static DefaultRenderDevice CreateDevice(SizeF pageSize)
        {
            // Simulate the screen size
            var width = (int)pageSize.Width.PtToPixel();
            var height = (int)pageSize.Height.PtToPixel();

            return HtmlSharedUtils.CreateRenderDevice(width, height);
        }

        private static string? CheckFontFile(string fontName)
        {
            string[] locations;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                locations = [
                    @"%SYSTEMROOT%\Fonts",
                    @"%APPDATA%\Microsoft\Windows\Fonts",
                    @"%LOCALAPPDATA%\Microsoft\Windows\Fonts"];
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                locations = [
                    "/usr/share/fonts",
                    "/usr/local/share/fonts",
                    "~/.fonts"];
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                locations = [
                    "/Library/Fonts",
                    "/System/Library/Fonts",
                    "~/Library/Fonts"];
            }
            else
            {
                locations = [];
            }

            foreach (var location in locations)
            {
                var fontFile = Path.Combine(Environment.ExpandEnvironmentVariables(location), fontName);
                if (File.Exists(fontFile))
                {
                    return fontFile;
                }
            }

            return null;
        }

        private static async Task LoadFontsAsync(PdfDocument pdf, IDocument document)
        {
            foreach (var sheet in document.StyleSheets)
            {
                var css = sheet as ICssStyleSheet;
                if (css == null)
                {
                    continue;
                }

                foreach (var rule in css.Rules)
                {
                    if (rule is ICssFontFaceRule fontFace)
                    {
                        var src = fontFace.GetPropertyValue(PropertyNames.Src);
                        var parts = src.Split(',', StringSplitOptions.RemoveEmptyEntries);
                        foreach (var part in parts)
                        {
                            if (part.StartsWith("local"))
                            {
                                var font = part[5..].Trim('(', ')', '"', '\'');
                                var file = CheckFontFile(font);
                                if (file == null) continue;

                                await pdf.Fonts.LoadAsync(file);

                                if (string.IsNullOrEmpty(pdf.Style.Font))
                                {
                                    pdf.Style.Font = fontFace.GetPropertyValue(PropertyNames.FontFamily).Trim('"', '\'');
                                }
                            }
                        }
                    }
                }
            }
        }

        private static async Task OnResourceRequested(RequestEvent evt)
        {
            if (evt.Response == null) return;

            var href = evt.Response.Address.Href;
            if (href.EndsWith(".css")) return;

            if (href.StartsWith("about:///")) href = href[9..];
            Resources[href] = await evt.Response.Content.ToBytesAsync();
        }

        private static PdfFontStyle ParseFontStyle(ICssStyleDeclaration css)
        {
            var fs = PdfFontStyle.Regular;

            var fontStyle = css.GetFontStyle();
            if (fontStyle == "italic" || fontStyle == "oblique")
            {
                fs |= PdfFontStyle.Italic;
            }

            var fontWeight = css.GetFontWeight();
            if (fontWeight == "bold" || fontWeight == "bolder" || (int.TryParse(fontWeight, out var fw) && fw >= 700))
            {
                fs |= PdfFontStyle.Bold;
            }

            return fs;
        }

        private static void SetPaddings(PdfStyle style, ICssStyleDeclaration css)
        {
            var padding = css.GetPointF(PropertyNames.Padding);
            if (padding.HasValue)
            {
                style.Padding = new PdfStyleSpace(padding.Value);
            }
            else
            {
                var paddingLeft = css.GetPointF(PropertyNames.PaddingLeft);
                var paddingTop = css.GetPointF(PropertyNames.PaddingTop);
                var paddingRight = css.GetPointF(PropertyNames.PaddingRight);
                var paddingBottom = css.GetPointF(PropertyNames.PaddingBottom);
                var paddings = new[] { paddingLeft, paddingTop, paddingRight, paddingBottom };
                if (paddings.Any(v => v.HasValue && v.Value != 0))
                {
                    style.Padding = new PdfStyleSpace(paddingLeft.GetValueOrDefault(), paddingTop.GetValueOrDefault(), paddingRight.GetValueOrDefault(), paddingBottom.GetValueOrDefault());
                }
            }
        }

        private static void SetRotate(PdfStyle style, ICssStyleDeclaration css)
        {
            var transform = css.GetProperty(PropertyNames.Transform);
            if (transform.RawValue is CssTupleValue<ICssValue> tt)
            {
                // Can improve here when the dependency's progress
                var item = tt.Items.FirstOrDefault(item => item is ICssTransformFunctionValue t && t.Name == "rotate");
                if (item != null)
                {
                    var rad = item.CssText.Split('(', ')')[1][..^3];
                    if (float.TryParse(rad, out var angle))
                    {
                        style.Rotate = angle;
                    }
                }
            }
        }

        private static void SetMargins(PdfStyle style, ICssStyleDeclaration css)
        {
            var margin = css.GetPointF(PropertyNames.Margin);
            if (margin.HasValue)
            {
                style.Margin = new PdfStyleSpace(margin.Value);
            }
            else
            {
                var marginLeft = css.GetPointF(PropertyNames.MarginLeft);
                var marginTop = css.GetPointF(PropertyNames.MarginTop);
                var marginRight = css.GetPointF(PropertyNames.MarginRight);
                var marginBottom = css.GetPointF(PropertyNames.MarginBottom);
                var margins = new[] { marginLeft, marginTop, marginRight, marginBottom };
                if (margins.Any(v => v.HasValue && v.Value != 0))
                {
                    style.Margin = new PdfStyleSpace(marginLeft.GetValueOrDefault(), marginTop.GetValueOrDefault(), marginRight.GetValueOrDefault(), marginBottom.GetValueOrDefault());
                }
            }
        }

        private static void SetBorders(PdfStyle style, ICssStyleDeclaration css)
        {
            var border = css.GetBorder();
            if (!string.IsNullOrEmpty(border))
            {
                style.SetBorder(border);
            }
            else
            {
                var borderLeft = css.GetBorderLeft();
                var borderRight = css.GetBorderRight();
                var borderTop = css.GetBorderTop();
                var borderBottom = css.GetBorderBottom();

                var borders = new[] { borderLeft, borderRight, borderTop, borderBottom };
                if (borders.Any(v => !string.IsNullOrEmpty(v)))
                {
                    var defaultColor = style.BorderColor ?? PdfColor.Black;
                    var defaultWidth = style.BorderWidth ?? 1;
                    var newBorder = new PdfStyleBorder(defaultColor, defaultWidth);

                    if (!string.IsNullOrEmpty(borderLeft))
                    {
                        var leftColor = PdfColor.Parse(css.GetBorderColor()) ?? defaultColor;
                        var leftWidth = css.GetPointF(PropertyNames.BorderLeftWidth) ?? defaultWidth;
                        var leftStyle = Enum.TryParse<PdfStyleBorderStyle>(css.GetBorderLeftStyle(), true, out var leftStyleItem) ? leftStyleItem : PdfStyleBorderStyle.Solid;
                        newBorder.Left = new PdfStyleBorder(leftColor, leftWidth, leftStyle);
                    }

                    if (!string.IsNullOrEmpty(borderTop))
                    {
                        var topColor = PdfColor.Parse(css.GetBorderColor()) ?? defaultColor;
                        var topWidth = css.GetPointF(PropertyNames.BorderTopWidth) ?? defaultWidth;
                        var topStyle = Enum.TryParse<PdfStyleBorderStyle>(css.GetBorderTopStyle(), true, out var topStyleItem) ? topStyleItem : PdfStyleBorderStyle.Solid;
                        newBorder.Top = new PdfStyleBorder(topColor, topWidth, topStyle);
                    }

                    if (!string.IsNullOrEmpty(borderRight))
                    {
                        var rightColor = PdfColor.Parse(css.GetBorderColor()) ?? defaultColor;
                        var rightWidth = css.GetPointF(PropertyNames.BorderRightWidth) ?? defaultWidth;
                        var rightStyle = Enum.TryParse<PdfStyleBorderStyle>(css.GetBorderRightStyle(), true, out var rightStyleItem) ? rightStyleItem : PdfStyleBorderStyle.Solid;
                        newBorder.Right = new PdfStyleBorder(rightColor, rightWidth, rightStyle);
                    }

                    if (!string.IsNullOrEmpty(borderBottom))
                    {
                        var bottomColor = PdfColor.Parse(css.GetBorderColor()) ?? defaultColor;
                        var bottomWidth = css.GetPointF(PropertyNames.BorderBottomWidth) ?? defaultWidth;
                        var bottomStyle = Enum.TryParse<PdfStyleBorderStyle>(css.GetBorderBottomStyle(), true, out var bottomStyleItem) ? bottomStyleItem : PdfStyleBorderStyle.Solid;
                        newBorder.Bottom = new PdfStyleBorder(bottomColor, bottomWidth, bottomStyle);
                    }
                }
            }
        }

        /// <summary>
        /// Update style from CSS declaration
        /// 从 CSS 声明更新样式
        /// </summary>
        /// <param name="style">PDF style</param>
        /// <param name="css">CSS declaration</param>
        /// <param name="root">Is root element</param>
        public static void UpdateFromDeclaration(this PdfStyle style, ICssStyleDeclaration css, bool root = false)
        {
            // Check font to avoid override default values
            var fontFamily = css.GetFontFamily();
            if (!string.IsNullOrEmpty(fontFamily))
            {
                style.Font = fontFamily;
            }

            var fontSize = css.GetPointF(PropertyNames.FontSize);
            if (fontSize.HasValue)
            {
                style.FontSize = fontSize.Value;
            }

            style.FontStyle = ParseFontStyle(css);

            style.BackgroundColor = PdfColor.Parse(css.GetBackgroundColor());
            style.Color = PdfColor.Parse(css.GetColor());
            style.Opacity = css.GetFloatValue(PropertyNames.Opacity).GetValueOrDefault(1);
            style.SetTextAlign(css.GetTextAlign());
            style.SetLineHeight(css.GetPointF(PropertyNames.LineHeight));
            style.WordSpacing = css.GetPointF(PropertyNames.WordSpacing);

            SetRotate(style, css);

            style.BorderColor = PdfColor.Parse(css.GetBorderColor());
            style.BorderWidth = css.GetPointF(PropertyNames.BorderWidth);

            SetPaddings(style, css);
            SetMargins(style, css);
            SetBorders(style, css);

            if (!root)
            {
                style.SetPosition(css.GetPosition());
                style.Width = css.GetPointF(PropertyNames.Width);
                style.Height = css.GetPointF(PropertyNames.Height);
                style.Left = css.GetPointF(PropertyNames.Left);
                style.Top = css.GetPointF(PropertyNames.Top);
            }
        }

        private static void SetMetaData(PdfDocument pdf, IDocument html, string? language)
        {
            // Metas
            var metas = html.Head?.QuerySelectorAll<IHtmlMetaElement>("meta");

            pdf.Metadata.Title = html.Title;
            pdf.Metadata.Author = metas?.FirstOrDefault(m => m.Name == "author")?.Content;
            pdf.Metadata.Subject = metas?.FirstOrDefault(m => m.Name == "description")?.Content;
            pdf.Metadata.Culture = new CultureInfo(language ?? "en");
            pdf.Metadata.Keywords = metas?.FirstOrDefault(m => m.Name == "keywords")?.Content;

            if (DateTime.TryParse(html.LastModified, out var lastModified))
            {
                pdf.Metadata.ModDate = lastModified;
            }
        }

        /// <summary>
        /// Parse PDF document from Html
        /// 从 Html 解析 PDF 文档
        /// </summary>
        /// <param name="htmlFile">HTML file path</param>
        /// <param name="pdfStream">PDF stream</param>
        /// <param name="pageSize">Page size, default is A4</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task</returns>
        public static async Task ParseAsync(string htmlFile, Stream pdfStream, SizeF? pageSize = null, CancellationToken cancellationToken = default)
        {
            await ParseAsync(File.OpenRead(htmlFile), Path.GetDirectoryName(htmlFile) ?? "", pdfStream, pageSize, cancellationToken);
        }

        /// <summary>
        /// Parse PDF document from Html
        /// 从 Html 解析 PDF 文档
        /// </summary>
        /// <param name="htmlStream">Html document stream</param>
        /// <param name="htmlRoot">HTML root path</param>
        /// <param name="pdfStream">PDF stream</param>
        /// <param name="pageSize">Page size, default is A4</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task</returns>
        public static async Task ParseAsync(Stream htmlStream, string htmlRoot, Stream pdfStream, SizeF? pageSize = null, CancellationToken cancellationToken = default)
        {
            // Page size
            var size = pageSize ?? PdfPageSize.A4;

            // Device
            var device = CreateDevice(size);

            var html = await HtmlParserExtended.CreateWithCssAndDownloadAsync(htmlStream, htmlRoot, OnResourceRequested, default, device, cancellationToken: cancellationToken);

            await CreateAsync(html, pdfStream, size, cancellationToken);
        }

        public static async Task ParseUrlAsync(string url, Stream pdfStream, SizeF? pageSize = null, CancellationToken cancellationToken = default)
        {
            // Page size
            var size = pageSize ?? PdfPageSize.A4;

            // Device
            var device = CreateDevice(size);

            var html = await HtmlParserExtended.CreateWithCssAndDownloadAsync(url, device, OnResourceRequested, cancellationToken: cancellationToken);

            await CreateAsync(html, pdfStream, size, cancellationToken);
        }

        public static async Task CreateAsync(IDocument html, Stream pdfStream, SizeF pageSize, CancellationToken cancellationToken)
        {
            // Body
            var body = html.Body ?? throw new ArgumentException("Body is required");

            // PDF document
            await using var pdf = new PdfDocument(pdfStream);

            // Load fonts
            await LoadFontsAsync(pdf, html);

            // Default page size
            pdf.PageData.PageSize = pageSize;

            // Default styles defined with 'html'
            var htmlStyle = html.DocumentElement.ComputeCurrentStyle();
            pdf.Style.UpdateFromDeclaration(htmlStyle, true);

            // Set metadata
            SetMetaData(pdf, html, body.Language);

            // Get writer and start writing
            var w = await pdf.GetWriterAsync((page) =>
            {
                // Page styles defined with 'body'
                var bodyStyle = body.ComputeCurrentStyle();
                page.Style.UpdateFromDeclaration(bodyStyle, true);
            });

            // Parse elements
            foreach (var element in body.Children)
            {
                var block = ParseElement(element);
                if (block == null)
                {
                    continue;
                }
                var pdfElement = await block.ParseAsync(cancellationToken);
                pdfElement.Style.UpdateFromDeclaration(element.ComputeCurrentStyle());
                await w.WriteAsync(pdfElement);
            }

            await pdf.DisposeAsync();
        }

        private static IHtmlBlock? ParseElement(IElement element)
        {
            var tag = element.TagName.ToLower();
            switch (tag)
            {
                case "div":
                    return new HtmlDiv(element);
                case "h1":
                case "h2":
                case "h3":
                case "h4":
                case "h5":
                case "h6":
                    return new HtmlHeading(element);
                case "hr":
                    return new HtmlHR(element);
                case "p":
                    return new HtmlParagraph(element);
                default:
                    break;
            }

            return null;
        }
    }
}
