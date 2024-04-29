using AngleSharp.Dom;
using com.etsoo.EasyPdf.Content;

namespace com.etsoo.EasyPdf.Html
{
    internal abstract class HtmlRichBlock<T> : HtmlBlock<T> where T : PdfRichBlock
    {
        public HtmlRichBlock(IElement element) : base(element)
        {

        }

        protected Task ParseChildrenAsync(T block, CancellationToken cancellationToken)
        {
            return ParseChildrenAsync(block, Element, cancellationToken);
        }

        private string TrimText(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return string.Empty;
            }

            if (text.StartsWith('\n') || text.StartsWith('\r'))
            {
                text = text.TrimStart(['\n', '\r', ' ']);
            }

            var index = text.LastIndexOf('\n');
            if (index > 0 && string.IsNullOrWhiteSpace(text[(index + 1)..]))
            {
                text = text[..index].TrimEnd('\n', '\r', ' ');
            }

            return text;
        }

        private async Task ParseChildrenAsync(T block, IElement element, CancellationToken cancellationToken)
        {
            foreach (var child in element.ChildNodes)
            {
                if (child is IElement el)
                {
                    var tag = el.TagName;
                    switch (tag)
                    {
                        case "A":
                            var a = new PdfLink(TrimText(child.TextContent), new Uri(el.GetAttribute("href") ?? "#"), el.GetAttribute("title"));
                            a.Style.UpdateFromDeclaration(el.ComputeCurrentStyle());
                            block.Add(a);
                            break;
                        case "BR":
                            var br = new PdfLineBreak();
                            block.Add(br);
                            break;
                        case "IMG":
                            var src = el.GetAttribute("src");
                            if (!string.IsNullOrEmpty(src) && PdfDocumentHtmlParser.Resources.TryGetValue(src, out var imageBytes))
                            {
                                var image = await PdfImage.LoadAsync(imageBytes, cancellationToken);
                                image.Style.UpdateFromDeclaration(el.ComputeCurrentStyle());
                                block.Add(image);
                            }
                            break;
                        case "SUB":
                            var sub = new PdfSubscript(TrimText(child.TextContent));
                            sub.Style.UpdateFromDeclaration(el.ComputeCurrentStyle());
                            block.Add(sub);
                            break;
                        case "SUP":
                            var sup = new PdfSuperscript(TrimText(child.TextContent));
                            sup.Style.UpdateFromDeclaration(el.ComputeCurrentStyle());
                            block.Add(sup);
                            break;
                        default:
                            await ParseChildrenAsync(block, el, cancellationToken);
                            continue;
                    }
                }
                else if (child.NodeType == NodeType.Text)
                {
                    var text = TrimText(child.TextContent);
                    if (string.IsNullOrEmpty(text))
                    {
                        continue;
                    }
                    var chunk = new PdfTextChunk(text);
                    chunk.Style.UpdateFromDeclaration(element.ComputeCurrentStyle());
                    block.Add(chunk);
                }
                else
                {
                    continue;
                }
            }
        }
    }
}
