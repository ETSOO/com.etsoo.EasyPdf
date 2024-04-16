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

pdf.Style.FontSize = 16;
pdf.Style.Border = new PdfStyleBorder(PdfColor.Red, 10);

// Fonts
//await pdf.Fonts.LoadAsync("C:\\Windows\\Fonts\\simsun.ttc");
//pdf.Style.Font = "宋体";

await pdf.Fonts.LoadAsync("C:\\Windows\\Fonts\\msyh.ttc");
pdf.Style.Font = "Microsoft YaHei"; // 微软雅黑

// Get writer and start writing
var w = await pdf.GetWriterAsync((page) =>
{
    page.Data.PageSize = PdfPageSize.A4;
    page.Style.LineHeight = 30;
});

// Paragraphs
var p0 = new PdfParagraph();
p0.Add(new PdfImage("D:\\etsoo.png"));
await w.WriteAsync(p0);

var p1 = new PdfParagraph();
p1.Style.SetFontSize(30).SetTextAlign(PdfTextAlign.Center).SetMargin(new PdfStyleSpace(10, 0));
p1.Add("亿速思维");
p1.Add(new PdfSuperscript("®"));
p1.Add(" - ");
p1.Add("ETSOO");
p1.Add(new PdfSuperscript("®"));
await w.WriteAsync(p1);

var p2 = new PdfParagraph();
p2.Style.FontStyle = PdfFontStyle.Bold;
p2.Add("青岛亿速思维网络科技有限公司 粗体");
await w.WriteAsync(p2);

var p3 = new PdfParagraph();
p3.Style.FontStyle = PdfFontStyle.Italic;
p3.Add("青岛亿速思维网络科技有限公司 斜体");
await w.WriteAsync(p3);

var p4 = new PdfParagraph();
p4.Style.FontStyle = PdfFontStyle.BoldItalic;
p4.Add("上海亿商网络科技有限公司 粗体 & 斜体");
await w.WriteAsync(p4);

var hr = new PdfHR();
await w.WriteAsync(hr);

var p5 = new PdfParagraph();
p5.Style.SetFontSize(14)
    .SetTextAlign(PdfTextAlign.Justify)
    .SetLineHeight(20)
    .SetBorder(new PdfStyleBorder(PdfColor.Blue, 2))
    .SetBackgroundColor(PdfColor.Parse("#f3f3f3"))
    .SetMargin(new PdfStyleSpace(20, 0))
    .SetPadding(new PdfStyleSpace(10));
p5.Add("亿速思维(ETSOO)自成立以来致力于自主研发，在过去的20年中一直秉持着对技术的不懈追求和创新精神，为中小企业提供高效的信息化管理解决方案。公司的使命在于为客户创造价值，通过持续创新和卓越的服务，助力企业实现数字化转型。ETSOO has been dedicated to independent research and development since its establishment, maintaining an unwavering pursuit of technology and spirit of innovation over the past 20 years. We specialize in providing efficient information management solutions for small and medium-sized enterprises. Our mission is to create value for our customers by facilitating digital transformation through continuous innovation and excellent service.");
await w.WriteAsync(p5);

var p6 = new PdfParagraph();
p6.Style.SetFontSize(50).SetColor(new PdfColor(255, 0, 0)).SetRotate(-45).SetOpacity(0.1f).SetTextAlign(PdfTextAlign.Center);
p6.Add($"ETSOO® 亿速思维® {DateTime.Now.Year}");
await w.WriteAsync(p6);

var p7 = new PdfParagraph();
p7.Style.SetFontSize(12).SetTextAlign(PdfTextAlign.End);
p7.Add("欢迎访问 / Please visit ");
p7.Add(new PdfLink("ETSOO Website", new Uri("https://www.etsoo.com"), "点击访问官方网站"));
await w.WriteAsync(p7);

// Dispose
await pdf.DisposeAsync();

Console.WriteLine("Done!");
Console.ReadLine();