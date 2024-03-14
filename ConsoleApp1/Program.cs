using com.etsoo.EasyPdf;
using com.etsoo.EasyPdf.Content;
using com.etsoo.EasyPdf.Support;

var path = "D:\\a.pdf";
File.Delete(path);

// PDF document
var stream = File.OpenWrite(path);
var pdf = new PdfDocument(stream);
pdf.Metadata.Title = "青岛亿速思维网络科技有限公司";

pdf.Style.FontSize = 12;

// Fonts
// await pdf.Fonts.LoadAsync("D:\\subset.ttf");
await pdf.Fonts.LoadAsync("C:\\Windows\\Fonts\\simsun.ttc");
pdf.Style.Font = "宋体";

//await pdf.Fonts.LoadAsync("C:\\Windows\\Fonts\\msyh.ttc");
//pdf.PageData.Font = "微软雅黑";

// Get writer and start writing
var w = await pdf.GetWriterAsync();
await w.NewPageAsync((page) =>
{
    page.Data.PageSize = PdfPageSize.A3;
});

// Paragraph
var p = new PdfParagraph();
p.Add("中国智造 - 青岛亿速思维网络科技有限公司");
await w.AddAsync(p);

var p1 = new PdfParagraph();
p1.Style.FontStyle = PdfFontStyle.Bold;
p1.Add("中国智造 - 青岛亿速思维网络科技有限公司 粗体");
await w.AddAsync(p1);

var p2 = new PdfParagraph();
p2.Style.FontStyle = PdfFontStyle.Italic;
p2.Add("中国智造 - 青岛亿速思维网络科技有限公司 斜体");
await w.AddAsync(p2);

// Close
await pdf.CloseAsync();

Console.WriteLine("Done!");
Console.ReadLine();