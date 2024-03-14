using AngleSharp.Html.Parser;

namespace com.etsoo.EasyPdf
{
    /// <summary>
    /// Html parser extension
    /// Html 解析器扩展
    /// </summary>
    public static class PdfDocumentHtmlParser
    {
        /// <summary>
        /// Parse PDF document from Html
        /// 从 Html 解析 PDF 文档
        /// </summary>
        /// <param name="htmlStream">Html document stream</param>
        /// <returns>Task</returns>
        public static async Task ParseFromHtmlAsync(Stream htmlStream)
        {
            var parser = new HtmlParser();
            var html = await parser.ParseDocumentAsync(htmlStream);
        }
    }
}
