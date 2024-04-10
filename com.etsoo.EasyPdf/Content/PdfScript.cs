
using com.etsoo.EasyPdf.Fonts;
using com.etsoo.EasyPdf.Support;
using System.Drawing;

namespace com.etsoo.EasyPdf.Content
{
    /// <summary>
    /// PDF superscript
    /// PDF 上标
    /// </summary>
    public class PdfSuperscript(ReadOnlySpan<char> content)
        : PdfScript(content, false)
    {
    }

    /// <summary>
    /// PDF subscript
    /// PDF 下标
    /// </summary>
    public class PdfSubscript(ReadOnlySpan<char> content)
        : PdfScript(content, true)
    {
    }

    public abstract class PdfScript : PdfTextChunk
    {
        private float offset = 0.5f;

        public PdfScript(ReadOnlySpan<char> content, bool sub)
            : base(content, sub ? PdfChunkType.Subscript : PdfChunkType.Superscript)
        {
        }

        protected override void SetupOperators(List<byte[]> operators, IPdfWriter writer)
        {
            if (Font != null)
            {
                operators.Add(writer.WriteFont(Font));
            }

            operators.Add(PdfOperator.Ts(offset));
        }

        private (IPdfFont, float?) GetNearestNormalFont(IPdfWriter writer)
        {
            var chunk = PreviousSibling;
            while (chunk != null)
            {
                if (chunk is not PdfScript && chunk is PdfTextChunk textChunk && textChunk.Font != null)
                {
                    return (textChunk.Font, textChunk.LineHeight);
                }

                chunk = chunk.PreviousSibling;
            }

            var style = Style.Parent?.GetComputedStyle();

            var family = style?.Font!;
            var size = (style?.FontSize).GetValueOrDefault(16).PxToPt();
            var fontStyle = style?.FontStyle ?? PdfFontStyle.Regular;

            var font = writer.CreateFont(family, size, fontStyle);
            return (font, style?.GetLineHeight(font.LineHeight));
        }

        public override async Task<bool> WriteAsync(IPdfWriter writer, RectangleF rect, PdfPoint point, PdfBlockLine line, Func<PdfBlockLine, PdfBlockLine, Task> newLineAction)
        {
            var (font, lineHeight) = GetNearestNormalFont(writer);
            var (size, offset) = Type == PdfChunkType.Subscript ? font.Subscript : font.Superscript;

            Font = font;
            LineHeight = lineHeight;

            this.offset = offset;

            if (Style.FontSize == null)
            {
                // Set script font size
                Style.FontSize = size;
            }

            return await base.WriteAsync(writer, rect, point, line, newLineAction);
        }
    }
}
