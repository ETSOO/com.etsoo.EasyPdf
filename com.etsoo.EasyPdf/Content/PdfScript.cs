
using com.etsoo.EasyPdf.Fonts;
using com.etsoo.EasyPdf.Objects;
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
        private float offset;

        public PdfScript(ReadOnlySpan<char> content, bool sub)
            : base(content, sub ? PdfChunkType.Subscript : PdfChunkType.Superscript)
        {
            Style.Inherit = false;
        }

        public override Task CalculatePositionAsync(IPdfPage page, PdfBlockLine line, PdfBlockLineChunk chunk)
        {
            var size = Font!.Size;
            var chunkSize = chunk.Font.Size;
            if (Type == PdfChunkType.Superscript)
            {
                chunk.StartPoint.Y += line.Height + 1 - size - offset / 2;
            }
            else
            {
                chunk.StartPoint.Y += line.Height - chunkSize - offset;
            }

            return base.CalculatePositionAsync(page, line, chunk);
        }

        private (IPdfFont, float?) GetNearestNormalFont(PdfWriter writer)
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

        public override async Task<bool> WriteAsync(PdfWriter writer, RectangleF rect, PdfPoint point, PdfBlockLine line, Func<PdfBlockLine, PdfBlockLine?, Task> newLineAction)
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

            if (PreviousSibling is PdfScript ps && ps.Type != Type)
            {
                var startPoint = ps.StartPoint;
                var endPoint = ps.EndPoint;

                point.X = startPoint.X;
                point.Y = startPoint.Y;

                var newPage = await base.WriteAsync(writer, rect, point, line, newLineAction);

                var newEndPoint = EndPoint;
                if (newEndPoint.Y > endPoint.Y || (newEndPoint.Y == endPoint.Y && newEndPoint.X > endPoint.X))
                {
                    point.X = newEndPoint.X;
                    point.Y = newEndPoint.Y;
                }
                else
                {
                    point.X = endPoint.X;
                    point.Y = endPoint.Y;
                }

                return newPage;
            }
            else
            {
                return await base.WriteAsync(writer, rect, point, line, newLineAction);
            }
        }
    }
}
