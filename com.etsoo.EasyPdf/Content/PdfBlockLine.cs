using com.etsoo.EasyPdf.Fonts;
using System.Numerics;

namespace com.etsoo.EasyPdf.Content
{
    /// <summary>
    /// PDF block element's line chunk
    /// PDF 块元素的行块
    /// </summary>
    public record PdfBlockLineChunk
    {
        /// <summary>
        /// Find bytes in the source list
        /// 查找源列表中的字节
        /// </summary>
        /// <param name="source">Source list</param>
        /// <param name="endBytes">Item ended with bytes</param>
        /// <returns>Index</returns>
        public static int FindBytes(List<byte[]> source, byte[] endBytes)
        {
            var len = endBytes.Length;
            if (len == 0)
            {
                return -1;
            }

            for (var i = 0; i < source.Count; i++)
            {
                if (source[i].Length < len)
                {
                    continue;
                }

                for (var e = 1; e <= len; e++)
                {
                    if (source[i][^e] != endBytes[^e])
                    {
                        break;
                    }

                    if (e == len)
                    {
                        return i;
                    }
                }
            }

            return -1;
        }

        /// <summary>
        /// Chunk owner
        /// 块所有者
        /// </summary>
        public required PdfChunk Owner { get; init; }

        /// <summary>
        /// Font
        /// 字体
        /// </summary>
        public IPdfFont? Font { get; }

        /// <summary>
        /// Blank character
        /// 空白字符
        /// </summary>
        public char? BlankChar { get; set; }

        /// <summary>
        /// End operators
        /// 结束操作符
        /// </summary>
        public List<byte[]> EndOperators { get; set; } = [];

        /// <summary>
        /// Operators
        /// 操作符
        /// </summary>
        public List<byte[]> Operators { get; set; } = [];

        /// <summary>
        /// Chars to draw
        /// 绘制的字符
        /// </summary>
        public List<char> Chars { get; set; } = [];

        /// <summary>
        /// Char widths
        /// 字符宽度
        /// </summary>
        public List<float> Widths { get; set; } = [];

        /// <summary>
        /// Chunk height
        /// 块高
        /// </summary>
        public float Height { get; }

        /// <summary>
        /// Start point
        /// 开始点
        /// </summary>
        public Vector2 StartPoint;

        /// <summary>
        /// Is sequence chunk
        /// 是否为序列块
        /// </summary>
        public bool IsSequence { get; set; }

        /// <summary>
        /// Font style
        /// 字体样式
        /// </summary>
        public PdfFontStyle FontStyle { get; }

        /// <summary>
        /// Computed style
        /// 计算的样式
        /// </summary>
        public required PdfStyle Style { get; init; }

        /// <summary>
        /// Constructor
        /// 构造函数
        /// </summary>
        /// <param name="font">Font</param>
        /// <param name="height">Height</param>
        /// <param name="startPoint">Start point</param>
        /// <param name="isSequence">Is sequence chunk</param>
        /// <param name="FontStyle">Font style</param>
        public PdfBlockLineChunk(IPdfFont? font, float height, Vector2 startPoint, bool isSequence, PdfFontStyle fontStyle = PdfFontStyle.Regular)
        {
            Font = font;
            Height = height;
            StartPoint = startPoint;
            IsSequence = isSequence;
            FontStyle = fontStyle;
        }

        /// <summary>
        /// Adjust the start point
        /// 调整开始点
        /// </summary>
        /// <param name="x">Adjust X</param>
        /// <param name="y">Adjust Y</param>
        public void AdjustStartPoint(float x, float y = 0)
        {
            StartPoint.X += x;
            StartPoint.Y += y;

            IsSequence = false;
        }

        /// <summary>
        /// Find operator
        /// 查找操作符
        /// </summary>
        /// <param name="endBytes">Position ended with the bytes</param>
        /// <returns>Index</returns>
        public int FindOperator(byte[] endBytes)
        {
            return FindBytes(Operators, endBytes);
        }

        /// <summary>
        /// Insert bytes to operators after the position
        /// 在指定位置后插入字节到操作符
        /// </summary>
        /// <param name="bytes">Bytes to insert</param>
        /// <param name="endBytes">Position ended with the bytes</param>
        /// <param name="avoidDupliate">Avoid duplicate item</param>
        /// <returns>Inserted index</returns>
        public int InsertAfter(byte[] bytes, byte[] endBytes, bool avoidDupliate = true)
        {
            var pos = FindBytes(Operators, endBytes);

            if (pos != -1)
            {
                pos++;

                if (pos == Operators.Count)
                {
                    Operators.Add(bytes);
                }
                else
                {
                    if (avoidDupliate)
                    {
                        var next = Operators[pos];
                        var len = Math.Min(next.Length, bytes.Length);

                        for (var i = 1; i <= len; i++)
                        {
                            if (next[^i] != bytes[^i])
                            {
                                if (i > 4)
                                {
                                    // Replace
                                    Operators[pos] = bytes;

                                    return pos;
                                }
                                else
                                {
                                    break;
                                }
                            }
                        }
                    }

                    Operators.Insert(pos, bytes);
                }
            }

            return pos;
        }
    }

    /// <summary>
    /// PDF block element's line
    /// PDF 块元素的行
    /// </summary>
    public record PdfBlockLine
    {
        /// <summary>
        /// Index
        /// 行索引
        /// </summary>
        public int Index { get; }

        /// <summary>
        /// Is first line
        /// 是否为第一行
        /// </summary>
        public bool First => Index == 0;

        /// <summary>
        /// Line width
        /// 行宽
        /// </summary>
        public float Width { get; set; }

        /// <summary>
        /// Line height, highest chunk's height
        /// 行高，最高块的高度
        /// </summary>
        public float Height { get; private set; }

        /// <summary>
        /// Max font height
        /// 最大字体高度
        /// </summary>
        public float MaxFontHeight { get; private set; }

        /// <summary>
        /// Is rendered
        /// 是否已渲染
        /// </summary>
        public bool Rendered { get; set; }

        /// <summary>
        /// Full width
        /// 完整宽度
        /// </summary>
        public bool FullWidth { get; set; }

        /// <summary>
        /// Total spaces
        /// 总空格数
        /// </summary>
        public int Spaces { get; set; }

        private readonly List<PdfBlockLineChunk> chunks = [];

        /// <summary>
        /// Chunks
        /// 块
        /// </summary>
        public PdfBlockLineChunk[] Chunks => [.. chunks];

        /// <summary>
        /// Has text
        /// 是否有文本
        /// </summary>
        public bool HasText => chunks.Any(c => c.Chars.Count > 0);

        internal PdfBlockLine(int index)
        {
            Index = index;
        }

        /// <summary>
        /// Add a chunk
        /// 添加块
        /// </summary>
        /// <param name="chunk">Line chunk</param>
        public void AddChunk(PdfBlockLineChunk chunk)
        {
            chunks.Add(chunk);

            if (chunk.Height > Height)
            {
                Height = chunk.Height;
            }

            if (chunk.Font != null)
            {
                var fontHeight = chunk.Font.LineHeight - chunk.Font.LineGap;
                if (fontHeight > MaxFontHeight)
                {
                    MaxFontHeight = fontHeight;
                }
            }
            else
            {
                // No font, like picture
                if (chunk.Height > MaxFontHeight)
                {
                    MaxFontHeight = chunk.Height;
                }
            }
        }
    }
}