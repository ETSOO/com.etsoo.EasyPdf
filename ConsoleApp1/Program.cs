// See https://aka.ms/new-console-template for more information
using com.etsoo.EasyPdf.Font;

Console.WriteLine("Start...");

var fontFolder = Environment.GetFolderPath(Environment.SpecialFolder.Fonts);

await using var fontStream = File.OpenRead($"{fontFolder}\\msyh.ttc");

await using var subset = await EasyFont.CreateSubsetAsync(fontStream, "中国智造，慧及全球");

await using var fileStream = File.OpenWrite("D:\\subset.ttf");

subset.Position = 0;
await subset.CopyToAsync(fileStream);

Console.WriteLine("Done!");