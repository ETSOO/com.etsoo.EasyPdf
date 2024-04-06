using com.etsoo.EasyPdf.Fonts;
using com.etsoo.EasyPdf.Support;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Numerics;

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

        private int index;
        private (char item, float width)[]? chars;

        /// <summary>
        /// Constructor
        /// 构造函数
        /// </summary>
        /// <param name="content">Content</param>
        public PdfTextChunk(ReadOnlySpan<char> content)
        {
            Memory<char> cache = new char[content.Length];
            content.CopyTo(cache.Span);
            Content = cache;
        }

        public override async Task<bool> WriteAsync(IPdfWriter writer, RectangleF rect, PdfPoint point, PdfBlockLine line, Func<PdfBlockLine, PdfBlockLine, Task> newLineAction)
        {
            // Computed style
            var style = Style.GetComputedStyle();

            // Operators
            var operators = new List<byte>();

            // Begin text
            operators.AddRange(PdfOperator.BT);

            // Save graphics state
            operators.AddRange(PdfOperator.q);

            // Font
            var (font, fontChanged) = writer.WriteFont(operators, style, true);

            // Line height
            var lineHeight = style.GetLineHeight(font.LineHeight);

            // Letter spacing
            var letterSpacing = style.LetterSpacing ?? 0;

            // Word spacing
            var wordSpacing = style.WordSpacing ?? 0;

            // Set styles
            var styleBytes = font.SetupStyle(style, out var fakeStyle);
            operators.AddRange(styleBytes);

            // Precalculate characters
            chars ??= font.Precalculate(this);

            // New page
            var newPage = false;

            var chunk = new PdfBlockLineChunk(font, lineHeight, point.ToVector2(), fakeStyle)
            {
                Operators = operators
            };
            line.Chunks.Add(chunk);

            //if (fakeStyle.HasFlag(PdfFontStyle.Bold))
            //{
            //    // Two sides
            //    var boldSize = 2 * PdfFontUtils.GetBoldSize(font.Size);
            //    line.Width += boldSize;
            //}

            //if (fakeStyle.HasFlag(PdfFontStyle.Italic))
            //{
            //    // One side
            //    var italicSize = PdfFontUtils.GetItalicSize(font.Size);
            //    point.X += italicSize;
            //    line.Width += italicSize;
            //}

            var lastBlankIndex = 0;

            do
            {
                var (item, width) = chars[index];

                var character = Content.Span[index];

                if (character == ' ')
                {
                    // ASCII blank
                    if (lastBlankIndex != -1 && lastBlankIndex + 1 < index)
                    {
                        line.Words++;
                    }

                    lastBlankIndex = index;

                    chunk.BlankChar = item;
                }

                if (point.X + width > rect.Width)
                {
                    var category = CharUnicodeInfo.GetUnicodeCategory(character);
                    if (category == UnicodeCategory.SpaceSeparator)
                    {
                        if (chunk.Chars.Count == index)
                        {
                            // Remove possible previous space
                            var prev = Content.Span[index - 1];
                            if (CharUnicodeInfo.GetUnicodeCategory(prev) == UnicodeCategory.SpaceSeparator)
                            {
                                chunk.Chars.RemoveAt(index - 1);
                                point.X -= width;
                                line.Width -= width;
                            }
                        }

                        // Reset the flag, no necessary to handle later
                        lastBlankIndex = -1;

                        // Skip the space
                        index++;
                        continue;
                    }
                    else if (category == UnicodeCategory.ClosePunctuation || category == UnicodeCategory.OtherPunctuation)
                    {
                        // Put the punctuation to the end instead of the start of the next line
                        Debug.WriteLine($"Punctuation char {character} at {point.X}");
                    }
                    else
                    {
                        // English words
                        if (line.Words > 1 && lastBlankIndex > 0)
                        {
                            // Characters
                            var bcount = index - lastBlankIndex;

                            // Deduct the width, include the space
                            var bwidth = chars.Skip(lastBlankIndex).Take(bcount).Sum(item => item.width);

                            point.X -= bwidth;
                            line.Width -= bwidth;

                            var start = chunk.Chars.Count - bcount;
                            chunk.Chars.RemoveRange(start, bcount);

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
                        Vector2? startPoint = null;
                        List<byte> newOperators = [];
                        if (chunk.StartPoint?.X > 0)
                        {
                            // Complete the previous chunk
                            CompleteChunk(chunk);

                            startPoint = point.ToVector2();
                            newOperators = operators;
                        }

                        // New line
                        var newLine = new PdfBlockLine();
                        chunk = new PdfBlockLineChunk(font, lineHeight, startPoint)
                        {
                            Operators = newOperators
                        };
                        newLine.Chunks.Add(chunk);

                        // Previous line action
                        line.FullWidth = true;
                        await newLineAction(line, newLine);

                        line = newLine;

                        continue;
                    }
                }

                chunk.Chars.Add(item);

                point.X += width;
                line.Width += width;

                index++;
            } while (index < chars.Length);

            // Complete the chunk
            CompleteChunk(chunk);

            return newPage;
        }

        private void CompleteChunk(PdfBlockLineChunk chunk)
        {
            // Restore graphics state
            chunk.EndOperators.AddRange(PdfOperator.Q);

            // End text
            chunk.EndOperators.AddRange(PdfOperator.ET);
        }
    }
}