using com.etsoo.EasyPdf.Font;
using QuestPDF.Drawing;
using QuestPDF.Infrastructure;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace com.etsoo.EasyPdf.Document
{
    /// <summary>
    /// EasyPdf setup font
    /// EasyPdf 初始化字体
    /// </summary>
    public class EasyPdfSetupFont
    {
        /// <summary>
        /// Font stream
        /// 字体流
        /// </summary>
        public Stream? Stream { get; set; }

        /// <summary>
        /// System font file name
        /// 系统字体文件名
        /// </summary>
        public string? SystemFont { get; set; }

        /// <summary>
        /// Font name to load
        /// 加载字体名称
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Custom font name
        /// 自定义字体名称
        /// </summary>
        public string? CustomName { get; set; }

        /// <summary>
        /// Subset characters
        /// 子集字符
        /// </summary>
        public IEnumerable<char>? SubsetChars { get; set; }
    }

    /// <summary>
    /// EasyPdf abstract document
    /// EasyPdf 抽象文档
    /// </summary>
    public abstract class EasyPdfDocument : IDocument
    {
        /// <summary>
        /// Set no cache
        /// 设置无缓存
        /// </summary>
        public static void SetNoCache()
        {
            SKGraphics.PurgeAllCaches();
            SKGraphics.SetResourceCacheTotalByteLimit(0);
            SKGraphics.SetFontCacheLimit(0);
        }

        /// <summary>
        /// Load system font
        /// 加载系统字体
        /// </summary>
        /// <param name="name">Font name</param>
        /// <returns>Stream</returns>
        public static Stream LoadSystemFont(string name)
        {
            var fontFolder = Environment.GetFolderPath(Environment.SpecialFolder.Fonts);
            return File.OpenRead($"{fontFolder}\\{name}");
        }

        /// <summary>
        /// Async setup
        /// 异步初始化
        /// </summary>
        /// <param name="fonts">Fonts</param>
        /// <returns>Task</returns>
        public virtual async Task SetupAsync(IEnumerable<EasyPdfSetupFont>? fonts)
        {
            if (fonts != null)
            {
                foreach (var font in fonts)
                {
                    var systemFont = font.SystemFont;
                    var fontStream = font.Stream;
                    if (string.IsNullOrEmpty(systemFont) && fontStream == null)
                    {
                        throw new Exception("Invalid Font Definition");
                    }

                    await using var stream = fontStream ?? LoadSystemFont(systemFont!);
                    var customName = font.CustomName;
                    if (font.SubsetChars == null)
                    {
                        if (string.IsNullOrEmpty(customName)) FontManager.RegisterFont(stream);
                        else FontManager.RegisterFontWithCustomName(customName, stream);
                    }
                    else
                    {
                        // Add all ASCII characters
                        var ascii = Enumerable.Range(0, 256).Select(i => (char)i).Where(c => !char.IsControl(c));
                        var chars = font.SubsetChars.Concat(ascii);

                        await using var subset = await EasyFont.CreateSubsetAsync(stream, chars, font.Name);

                        if (string.IsNullOrEmpty(customName)) FontManager.RegisterFont(subset);
                        else FontManager.RegisterFontWithCustomName(customName, subset);
                    }
                }
            }
        }

        /// <summary>
        /// Compose the document
        /// 编写文档
        /// </summary>
        /// <param name="container">Document container</param>
        public abstract void Compose(IDocumentContainer container);

        /// <summary>
        /// Get PDF meta data
        /// 获取 PDF 元数据
        /// </summary>
        /// <returns>Result</returns>
        public virtual DocumentMetadata GetMetadata()
        {
            var data = DocumentMetadata.Default;
            data.Producer = "ETSOO / 亿速思维 PDF API";
            return data;
        }
    }
}
