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
pdf.Style.Border = new PdfStyleBorder(PdfColor.Red, 8);

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
img.Style.SetHeight(40).SetOpacity(0.3f);
p1.Add(img);
await w.WriteAsync(p1);

var h1 = new PdfHeading(PdfHeading.Level.H1);
h1.Style.SetTextAlign(PdfTextAlign.Center);
h1.Add("亿速思维");
h1.Add(new PdfSuperscript("®"));
h1.Add(" - ");
h1.Add("ETSOO");
h1.Add(new PdfSuperscript("®"));
await w.WriteAsync(h1);

var p2 = new PdfParagraph();
p2.Add("青岛亿速思维\n网络科技有限公司 粗体").Style.SetFontStyle(PdfFontStyle.Bold);
p2.Add(PdfLineBreak.New);
p2.Add("青岛亿速思维网络科技有限公司 斜体").Style.SetFontStyle(PdfFontStyle.Italic);
p2.Add(PdfLineBreak.New);
p2.Add("上海亿商网络科技有限公司 粗体 & 斜体").Style.SetFontStyle(PdfFontStyle.BoldItalic);
await w.WriteAsync(p2);

var hr = new PdfHR();
await w.WriteAsync(hr);

var p5 = new PdfParagraph();
p5.Style.SetFontSize(10)
    .SetTextAlign(PdfTextAlign.Justify)
    .SetLineHeight(16)
    .SetBorder(PdfColor.Blue, 1)
    .SetBackgroundColor("#f3f3f3")
    .SetPadding(0);
p5.Add("亿速思维(ETSOO)自成立以来致力于自主研发，在过去的20年中一直秉持着对技术的不懈追求和创新精神，为中小企业提供高效的信息化管理解决方案。公司的使命在于为客户创造价值，通过持续创新和卓越的服务，助力企业实现数字化转型。ETSOO has been dedicated to independent research and development since its establishment, maintaining an unwavering pursuit of technology and spirit of innovation over the past 20 years. We specialize in providing efficient information management solutions for small and medium-sized enterprises. Our mission is to create value for our customers by facilitating digital transformation through continuous innovation and excellent service.");
await w.WriteAsync(p5);

var p6 = new PdfParagraph();
p6.Style.SetPosition(PdfPosition.Absolute)
    .SetTop(160)
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