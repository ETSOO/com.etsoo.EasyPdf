# com.etsoo.EasyPdf
ETSOO PDF Generation Library / 亿速思维 PDF 生成库

# Background
We were very confused about whether to reinvent the wheel (have learned a lot from others for sure), but in the end we had to choose to do so because of the popular components (https://github.com/itext/itextsharp [Deprecated], https://github.com/empira/PDFsharp [Too complex], https://github.com/QuestPDF/QuestPDF [Depends on SkiaSharp]) all have more or less flaws, such as the output file is too large, or the code is difficult to maintain. We chose to start with NET 8 because maturing a set of code requires a process and years of improvement.

- Previous work to mitigate large file size of QuestPDF with Chinese font: https://www.nuget.org/packages/com.etsoo.EasyPdf/1.0.7 but lost fake font styles until 2024/03/18.
- Previous investigation on generation and parser of PDF: https://github.com/ETSOO/com.etsoo.EasyPdfBuild. The new library only focuses on generation.

# Features (targets)
- Based on PDF 1.7 standards.
- The library is licensed under MIT and permanently free.
- Following similar HTML5 layout ideas, can easily generate PDF file from HTML content.
- The output file size is as small as possible (example file with Chinese below is 163KB only).
- Keep the feature / code simple, easy to read and understood.
	1. Only supports CIDFont based on TrueType.
	2. Only supports JPEG / PNG images.
	3. Turn on PdfDocument.Debug to check source PDF operators easily.

# Milestones
- 2022/06/14 Start exploring PDF standards and code examples.
- 2024/03/18 Kickoff reinventing the wheel with .NET 8 + AOT supports.
- 2024/04/25 A simple layout with text and images is ready.

# Quick example

A console example to demonstrate the library:

[![](https://github.com/ETSOO/com.etsoo.EasyPdf/blob/master/example.png)](https://www.etsoo.com)

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
// All sizes are in points (pt) (1/72 inch)
await using var pdf = new PdfDocument(stream);
pdf.Metadata.Title = "ETSOO ® (亿速思维 ®)";
pdf.Metadata.Author = "Garry X";
pdf.Metadata.Subject = "A quick guide to generate simple PDF file";
pdf.Metadata.Culture = new CultureInfo("zh-CN");
pdf.Metadata.Keywords = "ETSOO, PDF";

// Global styles
pdf.Style.FontSize = 12;
pdf.Style.Border = new PdfStyleBorder(PdfColor.Red, 6);

// Fonts
//await pdf.Fonts.LoadAsync("C:\\Windows\\Fonts\\simsun.ttc");
//pdf.Style.Font = "宋体";

await pdf.Fonts.LoadAsync("C:\\Windows\\Fonts\\msyh.ttc");
pdf.Style.Font = "Microsoft YaHei"; // 微软雅黑

// Get writer and start writing
var w = await pdf.GetWriterAsync((page) =>
{
    page.Data.PageSize = PdfPageSize.A4;
});

// Paragraphs
var p1 = new PdfDiv();
var img = await PdfImage.LoadAsync("D:\\etsoo.png");
img.Style.SetHeight(40).SetOpacity(0.9f);
p1.Add(img);
//p1.Style.SetBorder(PdfColor.Red).SetBackgroundColor("#f3f3f3").SetPadding(6);
await w.WriteAsync(p1);

var h1 = new PdfHeading(PdfHeading.Level.H1);
h1.Style.SetTextAlign(PdfTextAlign.Center);
h1.Add("亿速思维");
h1.Add(new PdfSuperscript("®"));
h1.Add(" - ");
h1.Add("ETSOO");
h1.Add(new PdfSuperscript("®"));
await w.WriteAsync(h1);

var hr = new PdfHR()
{
    Color = PdfColor.Red
};
hr.Style.SetOpacity(0.5f);
await w.WriteAsync(hr);

var p2 = new PdfParagraph();
p2.Add("青岛亿速思维\n网络科技有限公司 粗体").Style.SetFontStyle(PdfFontStyle.Bold);
p2.Add(PdfLineBreak.New);
p2.Add("青岛亿速思维网络科技有限公司 斜体").Style.SetFontStyle(PdfFontStyle.Italic);
p2.Add(PdfLineBreak.New);
p2.Add("上海亿商网络科技有限公司 粗体 & 斜体").Style.SetFontStyle(PdfFontStyle.BoldItalic);
await w.WriteAsync(p2);

var p5 = new PdfParagraph();
p5.Style.SetFontSize(10)
    .SetTextAlign(PdfTextAlign.Justify)
    .SetLineHeight(15)
    .SetBorder(PdfColor.Blue, 0.75f, PdfStyleBorderStyle.Dotted)
    .SetBackgroundColor("#f6f6f6")
    .SetPadding(6);
p5.Add("亿速思维(ETSOO)自成立以来致力于自主研发，在过去的20年中一直秉持着对技术的不懈追求和创新精神，为中小企业提供高效的信息化管理解决方案。公司的使命在于为客户创造价值，通过持续创新和卓越的服务，助力企业实现数字化转型。ETSOO has been dedicated to independent research and development since its establishment, maintaining an unwavering pursuit of technology and spirit of innovation over the past 20 years. We specialize in providing efficient information management solutions for small and medium-sized enterprises. Our mission is to create value for our customers by facilitating digital transformation through continuous innovation and excellent service.");
await w.WriteAsync(p5);

var p6 = new PdfParagraph();
p6.Style.SetPosition(PdfPosition.Absolute)
    .SetTop(200)
    .SetFontSize(36)
    .SetColor(new PdfColor(255, 0, 0))
    .SetOpacity(0.1f)
    .SetRotate(-45)
    .SetTextAlign(PdfTextAlign.Center);
p6.Add($"ETSOO® 亿速思维® {DateTime.Now.Year}");
await w.WriteAsync(p6);

var p7 = new PdfParagraph();
p7.Style.SetFontSize(9).SetTextAlign(PdfTextAlign.End);
p7.Add("欢迎访问 / Please visit ");
p7.Add(new PdfLink("ETSOO Website", new Uri("https://www.etsoo.com"), "点击访问官方网站"));
await w.WriteAsync(p7);

// Dispose
await pdf.DisposeAsync();

Console.WriteLine("Done!");
Console.ReadLine();
```