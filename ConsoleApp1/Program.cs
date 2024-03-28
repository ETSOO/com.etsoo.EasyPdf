using com.etsoo.EasyPdf;
using com.etsoo.EasyPdf.Content;
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
await pdf.Fonts.LoadAsync("C:\\Windows\\Fonts\\Arial.ttf");

pdf.Style.Font = "宋体";


await pdf.Fonts.LoadAsync("C:\\Windows\\Fonts\\msyh.ttc");
pdf.Style.Font = "Microsoft YaHei"; // 微软雅黑


// Get writer and start writing
var w = await pdf.GetWriterAsync();
await w.NewPageAsync((page) =>
{
    page.Style.Border = new PdfStyleBorder(PdfColor.Red, 1);
    page.Style.Padding = new PdfStyleSpace(10);
    page.Style.LineHeight = 30;
    //page.Data.PageSize = new System.Drawing.SizeF(595, 300);
});

var p3 = new PdfParagraph();
p3.Style.FontStyle = PdfFontStyle.Bold;
p3.Style.TextAlign = PdfTextAlign.Center;
p3.Add("青岛亿速思维网络科技有限公司 (ETSOO QD)");
var chunk = p3.Add("斜体");
chunk.Style.FontSize = 30;
chunk.Style.FontStyle = PdfFontStyle.Italic;
//p3.Add("恢复政策").Style.FontStyle = PdfFontStyle.Regular;
await w.WriteAsync(p3);

var p1 = new PdfParagraph();
p1.Add("青岛亿速思维网络科技有限公司 1");
p1.Style.TextAlign = PdfTextAlign.End;
await w.WriteAsync(p1);

var p2 = new PdfParagraph();
p2.Add("青岛亿速思维网络科技有限公司 2");
await w.WriteAsync(p2);

var longText = new PdfParagraph();
var longChunk = longText.Add("关于<甘肃>前首富阙文彬的财产执行情况，再起波澜。3月21日，成都武侯法院发布一则悬赏公告，内容提到李建秋申请执行阙文彬合同纠纷一案，法院责令被执行人阙文彬限期履行生效法律文书确定的义务，但是被执行人逾期未履行，法院决定悬赏查找被执行人财产。该案的执行标的将近1300万元，悬赏金为执行到位金额的10%。3月22日，记者了解到，该案件的被执行人阙文彬正是此前因涉及借贷纠纷的甘肃“前首富”，其持有的全部恒康医疗股份被杭州市下城区人民法院司法冻结，而在2021年，成都市中级人民法院也曾发布拍卖信息，将位于成都双流国际机场的两架公务机“湾流G550”和“湾流G450”进行司法拍卖，这两架飞机原属四川纵横航空有限公司，该公司由四川恒康发展有限责任公司100%控股，当时的实控人正是阙文彬。为何要悬赏执行？有法院系统专业人士告诉记者，悬赏执行，其实就是法院在执行过程中依据申请执行人申请，向社会发布悬赏公告，公开发布悬赏信息征集知情人提供被执行人财产线索，并在据此取得执行效果后向财产线索提供人支付悬赏金的执行措施，“有几种情况可以申请悬赏执行，一是法院穷尽执行措施无法查找被执行人可供执行财产的；被执行人下落不明或隐匿行踪，且无法查证被执行人财产状况的；被执行人名下的车辆或其他动产被人民法院查封等限制措施后未能实际控制的；被执行人有转移、隐匿财产行为或嫌疑的；其他需要实行悬赏执行的。” - In this example, Vector2Wrapper is a class that contains a Vector2 property that contains a Vector3 property Vector4. X");
//longChunk.Style.LineHeight = 40;

var superscript = longText.Add("123456789abcdefg");
superscript.Style.Color = PdfColor.Red;
//superscript.Style.TextStyle = PdfTextStyle.SuperScript;

//var subscript = longText.Add("3b");
//subscript.Style.TextStyle = PdfTextStyle.SubScript;

longText.Add("继续文字，看看是否中断！");

await w.WriteAsync(longText);

var englishText = new PdfParagraph();
englishText.Style.Font = "Arial";
var enchunk = englishText.Add("The TJ operator provides even more flexibility by letting you independently specify the space between letters. Instead of a string, TJ accepts an array of strings and numbers. When it encounters a string, TJ displays it just as Tj does. But when it encounters a number, it subtracts that value from the current horizontal text position.");
await w.WriteAsync(englishText);

/*
var p2 = new PdfParagraph();
p2.Style.FontStyle = PdfFontStyle.Bold;
p2.Add("青岛亿速思维网络科技有限公司 粗体");
await w.AddAsync(p2);

// Paragraphs
var p1 = new PdfParagraph();
p1.Add("青岛亿速思维网络科技有限公司");
p1.Add("青岛亿速思维网络科技有限公司 粗体").Style.FontStyle = PdfFontStyle.Bold;
p1.Add("青岛亿速思维网络科技有限公司 斜体").Style.FontStyle = PdfFontStyle.Italic;
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
*/

// Dispose
await pdf.DisposeAsync();

Console.WriteLine("Done!");
Console.ReadLine();