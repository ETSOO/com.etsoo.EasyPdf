using com.etsoo.EasyPdf.Fonts;
using com.etsoo.EasyPdf.Objects;
using com.etsoo.EasyPdf.Support;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;

namespace com.etsoo.EasyPdf.Content
{
    /// <summary>
    /// PDF text chunk
    /// PDF 文本块
    /// </summary>
    public class PdfTextChunk : PdfChunk
    {
        /// <summary>
        /// Content
        /// 内容
        /// </summary>
        public ReadOnlyMemory<char> Content { get; init; }

        /// <summary>
        /// Font
        /// 字体
        /// </summary>
        public IPdfFont? Font { get; protected set; }

        private int index;
        private (char item, float width)[]? chars;

        /// <summary>
        /// Constructor
        /// 构造函数
        /// </summary>
        /// <param name="content">Content</param>
        /// <param name="type">Chunk type</param>
        public PdfTextChunk(ReadOnlySpan<char> content, PdfChunkType type = PdfChunkType.Text) : base(type)
        {
            Memory<char> cache = new char[content.Length];
            content.CopyTo(cache.Span);
            Content = cache;
        }

        public override Task CalculatePositionAsync(IPdfPage page, PdfBlockLine line, PdfBlockLineChunk chunk)
        {
            var isItalic = chunk.FontStyle.HasFlag(PdfFontStyle.Italic);

            var point = page.CalculatePoint(chunk.StartPoint);

            var x = point.X;
            var y = point.Y;
            if (isItalic)
            {
                x += PdfFontUtils.GetItalicSize(chunk.Height) / 1.1f;
            }

            // Text destination
            var angle = isItalic ? PdfFontUtils.ItalicAngle : 0;
            var pointBytes = angle == 0 ? PdfOperator.Td(x, y) : PdfOperator.Tm(1, 0, angle, 1, x, y, false);
            chunk.InsertAfter(pointBytes, PdfOperator.q);

            // Lending
            if (Type == PdfChunkType.Text)
            {
                var lineHeight = line.Height;
                if (chunk.Font.LineHeight < lineHeight)
                {
                    var index = chunk.FindOperator(PdfOperator.TLBytes);
                    if (index != -1)
                    {
                        chunk.Operators[index] = PdfOperator.TL(lineHeight);
                    }
                }
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Local operators setup
        /// 本地操作设置
        /// </summary>
        /// <param name="operators">Operators</param>
        /// <param name="writer">Writer</param>
        protected virtual void SetupOperators(List<byte[]> operators, IPdfWriter writer)
        {
        }

        /// <summary>
        /// Write chunk
        /// 输出块
        /// </summary>
        /// <param name="writer">Writer</param>
        /// <param name="rect">Rectangle to output</param>
        /// <param name="point">Current point</param>
        /// <param name="line">Current line</param>
        /// <param name="newLineAction">New line action</param>
        /// <returns>New page needed?</returns>
        public override async Task<bool> WriteAsync(IPdfWriter writer, RectangleF rect, PdfPoint point, PdfBlockLine line, Func<PdfBlockLine, PdfBlockLine, Task> newLineAction)
        {
            // Computed style
            var style = Style.GetComputedStyle();

            // Operators
            var operators = new List<byte[]>
            {
                // Begin text
                PdfOperator.BT,

                // Save graphics state
                PdfOperator.q
            };

            // Local setup
            SetupOperators(operators, writer);

            // Font
            var (font, _) = writer.WriteFont(operators, style, true);

            // Line height
            float lineHeight;
            if (Font == null)
            {
                Font = font;
                lineHeight = style.GetLineHeight(font.LineHeight);
            }
            else
            {
                lineHeight = LineHeight ?? Font.LineHeight;
            }

            LineHeight ??= lineHeight;

            // Letter spacing
            var letterSpacing = style.LetterSpacing.GetValueOrDefault().PxToPt();

            // Word spacing
            var wordSpacing = style.WordSpacing.GetValueOrDefault().PxToPt();

            // Margin left
            var marginLeft = (style.Margin?.Left ?? 0).PxToPt();

            // Margin right
            var marginRight = (style.Margin?.Right ?? 0).PxToPt();

            // Set styles
            var styleBytes = font.SetupStyle(style, out var fakeStyle);
            operators.AddRange(styleBytes);

            // Precalculate characters
            chars ??= font.Precalculate(this);

            // New page
            var newPage = false;

            // Start point
            StartPoint = point.ToVector2();
            var chunk = new PdfBlockLineChunk(font, lineHeight, StartPoint, false, fakeStyle)
            {
                Owner = this,
                Operators = operators,
                Style = style
            };
            line.AddChunk(chunk);

            // Margin left
            point.X += marginLeft;
            line.Width += marginLeft;

            var lastBlankIndex = 0;

            do
            {
                var (item, width) = chars[index];

                var widthWithSpacing = width + letterSpacing;

                var character = Content.Span[index];

                if (character == ' ')
                {
                    // ASCII blank
                    if (lastBlankIndex != -1)
                    {
                        line.Spaces++;

                        line.Width += wordSpacing;
                        point.X += wordSpacing;
                    }

                    lastBlankIndex = index;

                    // Defferent chunk may have different font, so the blank character may be different
                    chunk.BlankChar = item;
                }

                if (point.X + width > rect.Width)
                {
                    var category = CharUnicodeInfo.GetUnicodeCategory(character);
                    if (category == UnicodeCategory.ClosePunctuation || category == UnicodeCategory.OtherPunctuation)
                    {
                        // Put the punctuation to the end instead of the start of the next line
                        Debug.WriteLine($"Punctuation char {character} at {point.X}");
                    }
                    else
                    {
                        if (category == UnicodeCategory.SpaceSeparator)
                        {
                            // Remove the last space
                            line.Spaces--;

                            line.Width -= wordSpacing;
                            point.X -= wordSpacing;

                            // Reset the flag, no necessary to handle later
                            lastBlankIndex = -1;

                            // Skip the space
                            index++;
                        }

                        // English words
                        if (line.Spaces > 0 && lastBlankIndex > 0)
                        {
                            // Characters
                            var bcount = index - lastBlankIndex;

                            // Deduct the width, include the space
                            var start = chunk.Widths.Count - bcount;
                            var bwidth = chunk.Widths.Skip(start).Take(bcount).Sum();

                            point.X -= bwidth;
                            line.Width -= bwidth;

                            chunk.Chars.RemoveRange(start, bcount);
                            chunk.Widths.RemoveRange(start, bcount);

                            // Remove the last space
                            line.Spaces--;

                            line.Width -= wordSpacing;
                            point.X -= wordSpacing;

                            // Next character after the space
                            index = lastBlankIndex + 1;
                        }

                        // Point.Y now is the current line's top
                        // Add line height to the bottom
                        point.Y += lineHeight;

                        // No more space to continue
                        if (point.Y + lineHeight - rect.Y > rect.Height)
                        {
                            // Indicate the line is full width
                            line.FullWidth = true;

                            newPage = true;
                            break;
                        }

                        // Reset
                        lastBlankIndex = 0;
                        point.X = 0;

                        // Distinguish the new line follwing same style or a new style line starts
                        List<byte[]> newOperators = [];
                        var isSequence = true;
                        if (chunk.StartPoint.X > 0)
                        {
                            // Complete the previous chunk
                            CompleteChunk(chunk);

                            isSequence = false;
                            newOperators = operators;
                        }

                        // New line
                        var newLine = new PdfBlockLine();
                        chunk = new PdfBlockLineChunk(font, lineHeight, point.ToVector2(), isSequence)
                        {
                            Owner = this,
                            Operators = newOperators,
                            Style = style
                        };
                        newLine.AddChunk(chunk);

                        // Previous line action
                        line.FullWidth = true;
                        await newLineAction(line, newLine);

                        line = newLine;

                        continue;
                    }
                }

                chunk.Chars.Add(item);
                chunk.Widths.Add(width);

                point.X += widthWithSpacing;
                line.Width += widthWithSpacing;

                index++;
            } while (index < chars.Length);

            // Margin right
            point.X += marginRight;
            line.Width += marginRight;

            // Complete the chunk
            CompleteChunk(chunk);

            // End point
            EndPoint = point.ToVector2();

            return newPage;
        }

        private void CompleteChunk(PdfBlockLineChunk chunk)
        {
            // Restore graphics state
            // End text
            chunk.EndOperators.AddRange([PdfOperator.Q, PdfOperator.ET]);
        }
    }
}