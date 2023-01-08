// See https://aka.ms/new-console-template for more information
using com.etsoo.EasyPdf.Font;
using ConsoleApp1;
using QuestPDF.Fluent;

Console.WriteLine("Start...");

var fontFolder = Environment.GetFolderPath(Environment.SpecialFolder.Fonts);

// simsun.ttc
await using var fontStream = File.OpenRead($"{fontFolder}\\msyh.ttc");
//await using var fontStream = File.OpenRead($"D:\\subset.ttf");

// Add all ASCII characters
var ascii = Enumerable.Range(0, 256).Select(i => (char)i).Where(c => !char.IsControl(c));
var chars = "中国智造，慧及全球，中文，青岛亿速思维网络科技有限公司".Concat(ascii);
await using var subset = await EasyFont.CreateSubsetAsync(fontStream, chars);

await using var fileStream = File.OpenWrite("D:\\subset.ttf");

await subset.CopyToAsync(fileStream);

await fileStream.DisposeAsync();

Console.WriteLine("Create PDF...");
var document = new TestDocument();
await document.SetupAsync();
document.GeneratePdf("D:\\test.pdf");

Console.WriteLine("Done!");