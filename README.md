# com.etsoo.EasyPdf
ETSOO PDF Generation Library / 亿速思维 PDF 生成库

# Background
We were very confused about whether to reinvent the wheel (have learned a lot from others for sure), but in the end we had to choose to do so because of the popular components (https://github.com/itext/itextsharp [Deprecated], https://github.com/empira/PDFsharp [Too complex], https://github.com/QuestPDF/QuestPDF [Depends on SkiaSharp]) all have more or less flaws, such as the output file is too large, or the code is difficult to maintain. We chose to start with NET 8 because maturing a set of code requires a process and years of improvement.

- Previous work to mitigate large file size of QuestPDF with Chinese font: https://www.nuget.org/packages/com.etsoo.EasyPdf/1.0.7 but lost fake font styles until 2024/03/18.
- Previous investigation on generation and parser of PDF: https://github.com/ETSOO/com.etsoo.EasyPdfBuild. The new library only focuses on generation.

# Features (targets)
- Based on PDF 1.7 standards.
- The library is licensed under MIT and permanently free.
- Following HTML5 layout ideas, can easily generate PDF file from HTML content.
- The output file size is as small as possible.
- Keep the code simple, easy to read and understood.

# Milestones
- 2024/03/18 Reinventing wheel starts with .NET 8 + AOT supports and render simple text.

# Examples
Simple console example to demonstrate the library:
```csharp
using com.etsoo.EasyPdf;
using com.etsoo.EasyPdf.Content;
using com.etsoo.EasyPdf.Support;
using System.Globalization;

var path = "D:\\a.pdf";
File.Delete(path);

// Stream to write
var stream = File.OpenWrite(path);

// Turn on debug model
PdfDocument.Debug = true;

// PDF document
await using var pdf = new PdfDocument(stream);
pdf.Metadata.Title = "ETSOO ® (亿速思维 ®)";
pdf.Metadata.Author = "Garry X";
pdf.Metadata.Subject = "A quick guide to generate simple PDF file";
pdf.Metadata.Culture = new CultureInfo("zh-CN");
pdf.Metadata.Keywords = "ETSOO, PDF";

pdf.Style.FontSize = 12;
pdf.Style.BackgroundColor = PdfColor.Parse("gray");
pdf.Style.Border = new PdfStyleBorder(PdfColor.Red);

// Fonts
await pdf.Fonts.LoadAsync("C:\\Windows\\Fonts\\simsun.ttc");
pdf.Style.Font = "宋体";

/*
await pdf.Fonts.LoadAsync("C:\\Windows\\Fonts\\msyh.ttc");
pdf.Style.Font = "Microsoft YaHei"; // 微软雅黑
*/

// Get writer and start writing
var w = await pdf.GetWriterAsync();
await w.NewPageAsync((page) =>
{
    page.Data.PageSize = PdfPageSize.A4;
});

// Paragraphs
var p1 = new PdfParagraph();
p1.Add("青岛亿速思维网络科技有限公司");
await w.AddAsync(p1);

var p2 = new PdfParagraph();
p2.Style.FontStyle = PdfFontStyle.Bold;
p2.Add("青岛亿速思维网络科技有限公司 粗体");
await w.AddAsync(p2);

var p3 = new PdfParagraph();
p3.Style.FontStyle = PdfFontStyle.Italic;
p3.Add("青岛亿速思维网络科技有限公司 斜体");
await w.AddAsync(p3);

await w.NewPageAsync((page) =>
{
    page.Data.PageSize = PdfPageSize.A5;
    page.Style.FontSize = 16;
});

var p4 = new PdfParagraph();
p4.Style.FontStyle = PdfFontStyle.BoldItalic;
p4.Add("Qingdao ETSOO Network-Tech Co., Ltd. (Bold, Italic)");
await w.AddAsync(p4);

// Dispose
await pdf.DisposeAsync();

Console.WriteLine("Done!");
Console.ReadLine();
```