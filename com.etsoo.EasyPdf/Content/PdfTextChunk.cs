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

            // Save graphics state
            operators.AddRange(PdfOperator.q);

            // Font
            var (font, fontChanged) = writer.WriteFont(operators, style);

            // Line height
            var lineHeight = style.GetLineHeight(font.LineHeight);

            // Letter spacing
            var letterSpacing = style.LetterSpacing ?? 0;

            // Word spacing
            var wordSpacing = style.WordSpacing ?? 0;

            // Set styles
            var pointY = fontChanged ? (rect.Y + lineHeight - point.Y) : 0;
            var styleBytes = font.SetupStyle(style, new Vector2(point.X, pointY), out var fakeStyle);
            operators.AddRange(styleBytes);

            // Precalculate characters
            chars ??= font.Precalculate(this);

            // New page
            var newPage = false;

            var chunk = new PdfBlockLineChunk(font, lineHeight)
            {
                Operators = operators
            };
            line.Chunks.Add(chunk);

            if (fakeStyle.HasFlag(PdfFontStyle.Bold))
            {
                // Two sides
                var boldSize = 2 * PdfFontUtils.GetBoldSize(font.Size);
                line.Width += boldSize;
            }

            if (fakeStyle.HasFlag(PdfFontStyle.Italic))
            {
                // One side
                var italicSize = PdfFontUtils.GetItalicSize(font.Size);
                point.X += italicSize;
                line.Width += italicSize;
            }

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

                            // Deduct the width
                            var bwidth = chars.Skip(lastBlankIndex).Take(bcount).Sum(item => item.width);

                            point.X -= bwidth;
                            line.Width -= bwidth;

                            var start = chunk.Chars.Count - bcount;
                            chunk.Chars.RemoveRange(start, bcount);

                            // Next character after the space
                            index = lastBlankIndex + 1;

                            // Replace the current character item
                            item = chars[index].item;
                            character = Content.Span[index];
                        }

                        // No more space to continue
                        if (point.Y + lineHeight - rect.Y > rect.Height)
                        {
                            newPage = true;
                            break;
                        }

                        // New line
                        var newLine = new PdfBlockLine();
                        chunk = new PdfBlockLineChunk(font, lineHeight);
                        newLine.Chunks.Add(chunk);

                        // Previous line action
                        line.FullWidth = true;
                        await newLineAction(line, newLine);

                        // Reset
                        lastBlankIndex = 0;
                        point.X = 0;

                        // Next line
                        point.Y += lineHeight;

                        line = newLine;
                    }
                }

                chunk.Chars.Add(item);

                point.X += width;
                line.Width += width;

                index++;
            } while (index < chars.Length);

            // Restore graphics state
            chunk.EndOperators.AddRange(PdfOperator.Q);

            return newPage;
        }
    }
}