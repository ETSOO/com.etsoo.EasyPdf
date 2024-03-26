using com.etsoo.EasyPdf.Content;
using System.Numerics;
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
            return size * 0.1f;
        }

        public static float GetBoldSize(float size)
        {
            return size / BoldFactor;
        }

        public static float GetItalicSize(float size)
        {
            return size * ItalicAngle;
        }

        public static List<byte> SetupStyle(this IPdfFont font, PdfStyle style, Vector2 point, out PdfFontStyle fontStyle)
        {
            var bytes = new List<byte>();

            // Font color
            if (style.Color != null)
            {
                bytes.AddRange(RG2(style.Color.Value));
            }

            fontStyle = PdfFontStyle.Regular;

            var lineHeight = style.GetLineHeight(font.LineHeight);

            var setPoint = false;
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
                    // Plus 1.3F is from testing
                    bytes.AddRange(Tm(1, 0, ItalicAngle, 1, point.X + GetItalicSize(font.Size), lineHeight + 1.3F));
                    setPoint = true;

                    fontStyle |= PdfFontStyle.Italic;
                }
            }

            if (!setPoint)
            {
                bytes.AddRange(Tm(1, 0, 0, 1, point.X, lineHeight));
            }

            return bytes;
        }
    }
}
