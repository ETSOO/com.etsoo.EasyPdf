namespace com.etsoo.EasyPdf.Content
{
    /// <summary>
    /// PDF color
    /// PDF 颜色
    /// </summary>
    public readonly struct PdfColor
    {
        /// <summary>
        /// White
        /// 白色
        /// </summary>
        public static readonly PdfColor White = new(255, 255, 255);

        /// <summary>
        /// Black
        /// 黑色
        /// </summary>
        public static readonly PdfColor Black = new(0, 0, 0);

        /// <summary>
        /// Red
        /// 红色
        /// </summary>
        public static readonly PdfColor Red = new(255, 0, 0);

        /// <summary>
        /// Green
        /// 绿色
        /// </summary>
        public static readonly PdfColor Green = new(0, 255, 0);

        /// <summary>
        /// Blue
        /// 蓝色
        /// </summary>
        public static readonly PdfColor Blue = new(0, 0, 255);

        /// <summary>
        /// Yellow
        /// 黄色
        /// </summary>
        public static readonly PdfColor Yellow = new(255, 255, 0);

        /// <summary>
        /// Orange
        /// 橙色
        /// </summary>
        public static readonly PdfColor Orange = new(255, 200, 0);

        /// <summary>
        /// Pink
        /// 粉色
        /// </summary>
        public static readonly PdfColor Pink = new(255, 175, 175);

        /// <summary>
        /// Gray
        /// 灰色
        /// </summary>
        public static readonly PdfColor Gray = new(128, 128, 128);

        /// <summary>
        /// Purple
        /// 紫色
        /// </summary>
        public static readonly PdfColor Purple = new(160, 32, 240);

        /// <summary>
        /// Brown
        /// 棕色
        /// </summary>
        public static readonly PdfColor Brown = new(150, 75, 128);

        /// <summary>
        /// Red component value
        /// </summary>
        public byte R { get; }

        /// <summary>
        /// Green component value
        /// </summary>
        public byte G { get; }

        /// <summary>
        /// Blue component value
        /// </summary>
        public byte B { get; }

        public static PdfColor? Parse(string colorText)
        {
            colorText = colorText.Trim().ToLower();

            bool shortCase;
            if (colorText.StartsWith("#") && ((shortCase = colorText.Length == 4) || colorText.Length == 7))
            {
                colorText = colorText[1..];
                if (shortCase) colorText += colorText;
                var r = Convert.ToByte(colorText[..2], 16);
                var g = Convert.ToByte(colorText[2..4], 16);
                var b = Convert.ToByte(colorText[4..6], 16);
                return new PdfColor(r, g, b);
            }

            return colorText switch
            {
                "white" => White,
                "black" => Black,
                "red" => Red,
                "green" => Green,
                "blue" => Blue,
                "yellow" => Yellow,
                "orange" => Orange,
                "pink" => Pink,
                "gray" => Gray,
                "purple" => Purple,
                "brown" => Brown,
                _ => null
            };
        }

        public PdfColor(byte r, byte g, byte b)
        {
            R = r;
            G = g;
            B = b;
        }

        /// <summary>
        /// To PDF color string
        /// like "226, 24, 33" => ".8863 .0941 .1294"
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"{R / 255.0f} {G / 255.0f} {B / 255.0f}";
        }
    }
}
