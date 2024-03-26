using com.etsoo.EasyPdf.Fonts;

namespace com.etsoo.EasyPdf.Content
{
    /// <summary>
    /// PDF block element's line chunk
    /// PDF 块元素的行块
    /// </summary>
    public record PdfBlockLineChunk
    {
        /// <summary>
        /// Font
        /// 字体
        /// </summary>
        public IPdfFont Font { get; }

        /// <summary>
        /// End operators
        /// 结束操作符
        /// </summary>
        public List<byte> EndOperators { get; set; } = [];

        /// <summary>
        /// Operators
        /// 操作符
        /// </summary>
        public List<byte> Operators { get; set; } = [];

        /// <summary>
        /// Chars to draw
        /// 绘制的字符
        /// </summary>
        public List<char> Chars { get; set; } = [];

        /// <summary>
        /// Chunk height
        /// 块高
        /// </summary>
        public float Height { get; }

        /// <summary>
        /// Constructor
        /// 构造函数
        /// </summary>
        /// <param name="font">Font</param>
        /// <param name="height">Height</param>
        public PdfBlockLineChunk(IPdfFont font, float height)
        {
            Font = font;
            Height = height;
        }
    }

    /// <summary>
    /// PDF block element's line
    /// PDF 块元素的行
    /// </summary>
    public record PdfBlockLine
    {
        /// <summary>
        /// Line width
        /// 行宽
        /// </summary>
        public float Width { get; set; }

        /// <summary>
        /// Line height, highest chunk's height
        /// 行高，最高块的高度
        /// </summary>
        public float Height => Chunks.Max(c => c.Height);

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
        /// Total words
        /// 总单词数
        /// </summary>
        public int Words { get; set; }

        /// <summary>
        /// Chunks
        /// 块
        /// </summary>
        public List<PdfBlockLineChunk> Chunks = [];
    }
}
