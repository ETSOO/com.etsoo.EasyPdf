using com.etsoo.EasyPdf.Document;
using QuestPDF;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace ConsoleApp1
{
    internal class TestDocument : EasyPdfDocument
    {
        public async Task SetupAsync()
        {
            Settings.CheckIfAllTextGlyphsAreAvailable = false;
            Settings.EnableCaching = true;
            Settings.EnableDebugging = false;

            var fonts = new[] {
                new EasyPdfSetupFont { SystemFont = "msyh.ttc", SubsetChars = "UnderlineItalicBoldText2X-Y 中文" }
            };
            await base.SetupAsync(fonts);
        }

        public override void Compose(IDocumentContainer container)
        {
            container
                .Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(1.5f, Unit.Centimetre);
                    page.PageColor(Colors.White);

                    // Microsoft YaHei, SimSum
                    page.DefaultTextStyle(x => x.FontSize(12).FontFamily("Microsoft YaHei"));

                    page.Content().Text(text =>
                    {
                        text.Line("Bold Text 中文").Bold();
                        text.Line("Italic Text 中文").Italic();
                        text.Line("Underline Text").Underline();
                        text.Span("X");
                        text.Span("2").Subscript();
                        text.Span(" - Y");
                        text.Span("2").Superscript();
                    });

                    page.Footer().AlignCenter().Text(text =>
                    {
                        text.CurrentPageNumber();
                        text.Span(" / ");
                        text.TotalPages();
                    });
                });
        }
    }
}
