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

/*
pdf.Style.BackgroundColor = PdfColor.Parse("gray");
pdf.Style.Border = new PdfStyleBorder(PdfColor.Red, 40);
pdf.Style.Border.Top.Width = 80;
pdf.Style.Border.Bottom.Width = 160;
pdf.Style.Border.Bottom.Color = PdfColor.Yellow;
pdf.Style.Border.Right.Width = 120;
pdf.Style.Border.Right.Color = PdfColor.Blue;
*/

// Fonts
await pdf.Fonts.LoadAsync("C:\\Windows\\Fonts\\simsun.ttc");
pdf.Style.Font = "宋体";

//await pdf.Fonts.LoadAsync("C:\\Windows\\Fonts\\msyh.ttc");
//pdf.Style.Font = "Microsoft YaHei"; // 微软雅黑

// Get writer and start writing
var w = await pdf.GetWriterAsync();
await w.NewPageAsync((page) =>
{
    page.Style.Border = new PdfStyleBorder(PdfColor.Red, 1);
    page.Style.Padding = new PdfStyleSpace(30);
    page.Data.PageSize = PdfPageSize.A4;
});

var p3 = new PdfParagraph();
p3.Style.FontStyle = PdfFontStyle.Italic;
p3.Add("青岛亿速思维网络科技有限公司 (ETSOO QD) 斜体");
await w.AddAsync(p3);

var p2 = new PdfParagraph();
p2.Style.FontStyle = PdfFontStyle.Bold;
p2.Add("青岛亿速思维网络科技有限公司 粗体");
await w.AddAsync(p2);

// Paragraphs
var p1 = new PdfParagraph();
p1.Add("青岛亿速思维网络科技有限公司");
await w.AddAsync(p1);

var p5 = new PdfParagraph();
p5.Style.FontStyle = PdfFontStyle.BoldItalic;
p5.Add("青岛亿速思维网络科技有限公司 粗体斜体");
await w.AddAsync(p5);

await w.NewPageAsync((page) =>
{
    page.Data.PageSize = PdfPageSize.A4;
    page.Style.Padding = new PdfStyleSpace(30);
    page.Style.Color = PdfColor.Parse("white");
    page.Style.BackgroundColor = PdfColor.Parse("blue");
});

var p4 = new PdfParagraph();
p4.Style.FontStyle = PdfFontStyle.Bold;
p4.Add("Qingdao ETSOO Network-Tech Co., Ltd. (Bold, Italic)");
await w.AddAsync(p4);

// Dispose
await pdf.DisposeAsync();

Console.WriteLine("Done!");
Console.ReadLine();