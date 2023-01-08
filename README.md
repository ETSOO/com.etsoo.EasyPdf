# com.etsoo.EasyPdf
ETSOO PDF Library / 亿速思维PDF开发库

When working on PDF creation with QuestPDF (https://www.questpdf.com/), it is a result of the default behaviour of font embedding in SkiaSharp that cannot be subsetted. The issue (https://github.com/QuestPDF/QuestPDF/issues/31) will cause very big file generated even for a simple lines of PDF with Chinese or other similar fonts. That's reasonable but not acceptable.

Depending on my recent months of efforts to understand PDF, I have decided to develop a easy way to deal with Font subset. That's the result of the repository. And HTML to PDF features will also be added in the comming months.

# Example

```csharp
var fontFolder = Environment.GetFolderPath(Environment.SpecialFolder.Fonts);
await using var fontStream = File.OpenRead($"{fontFolder}\\msyh.ttc");
await using var subset = await EasyFont.CreateSubsetAsync(fontStream, "青岛亿速思维网络科技有限公司");

FontManager.RegisterFont(subset)
// FontManager.RegisterFontWithCustomName("msyh", subset);

// When create a page
page.DefaultTextStyle(x => x.FontSize(12).Fallback(f => f.FontFamily("Microsoft YaHei")));
```

With the font "Microsoft YaHei" subset, file size reduced to 125K from 11.8M. The only tricky thing is to generate the included characters.

1. 2023/01/07, found a similiar issue of https://github.com/QuestPDF/QuestPDF/issues/162. Bold / italic styles have no effect after applying the feature.
