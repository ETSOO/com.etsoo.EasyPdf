using com.etsoo.EasyPdf.Content;
using static com.etsoo.EasyPdf.Content.PdfOperator;

namespace com.etsoo.EasyPdf.Fonts
{
    internal static class PdfFontUtils
    {
        public const float BoldFactor = 30;
        public const float ItalicAngle = 0.21256f;

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
            return size * 0.2f;
        }

        public static float GetBoldSize(float size)
        {
            return size / BoldFactor;
        }

        public static float GetItalicSize(float size)
        {
            return size * ItalicAngle;
        }

        public static List<byte> SetupStyle(this IPdfFont font, PdfStyle style, out PdfFontStyle fontStyle)
        {
            var bytes = new List<byte>();

            // Font color
            if (style.Color != null)
            {
                bytes.AddRange(RG2(style.Color.Value));
            }

            fontStyle = PdfFontStyle.Regular;

            if (!font.IsMatch && font.Style != PdfFontStyle.Regular)
            {
                var mode = TrMode.Fill;

                if (font.Style.HasFlag(PdfFontStyle.Bold))
                {
                    bytes.AddRange(Zw(GetBoldSize(font.Size)));
                    mode = TrMode.FillThenStroke;

                    fontStyle |= PdfFontStyle.Bold;
                }

                bytes.AddRange(Tr(mode));

                if (font.Style.HasFlag(PdfFontStyle.Italic))
                {
                    // a, b [1, 0]
                    // c, d [0.21256, 1]
                    // e, f [0, 0]
                    fontStyle |= PdfFontStyle.Italic;
                }
            }

            return bytes;
        }
    }
}
