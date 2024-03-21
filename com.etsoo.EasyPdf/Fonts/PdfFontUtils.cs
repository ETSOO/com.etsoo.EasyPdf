using com.etsoo.EasyPdf.Content;

namespace com.etsoo.EasyPdf.Fonts
{
    internal static class PdfFontUtils
    {
        public static PdfSizeAndOffset GetSubscript(float size)
        {
            return new PdfSizeAndOffset(size / 2, -size / 10);
        }

        public static PdfSizeAndOffset GetSuperscript(float size)
        {
            return new PdfSizeAndOffset(size / 2, size / 2);
        }

        public static float GetLineGap(float size)
        {
            return size * 0.1f;
        }

        public static async ValueTask SetStyleAsync(this IPdfFont font, Stream stream)
        {
            if (font.IsMatch || font.Style == PdfFontStyle.Regular)
            {
                return;
            }

            // Artificial draw style
            await PdfOperator.SetupStyle(stream, font.Style, font.Size);
        }
    }
}
