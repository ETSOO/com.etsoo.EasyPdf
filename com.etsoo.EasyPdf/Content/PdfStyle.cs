using System.Drawing;
using System.Numerics;

namespace com.etsoo.EasyPdf.Content
{
    /// <summary>
    /// PDF style, all length units are in pixels
    /// PDF 样式，所有长度单位均为像素
    /// </summary>
    /// <remarks>
    /// Constructor
    /// 构造函数
    /// </remarks>
    /// <param name="parent">Parent style</param>
    public class PdfStyle(PdfStyle? parent = null)
    {
        public const string PositonAbsolte = "absolute";
        public const string PositonRelative = "relative";

        /// <summary>
        /// Parent style
        /// 父样式
        /// </summary>
        public PdfStyle? Parent { get; set; } = parent;

        /// <summary>
        /// Background color
        /// 背景颜色
        /// </summary>
        public PdfColor? BackgroundColor { get; set; }

        /// <summary>
        /// Border
        /// 边框
        /// </summary>
        public PdfStyleBorder? Border { get; set; }

        /// <summary>
        /// Color
        /// 颜色
        /// </summary>
        public PdfColor? Color { get; set; }

        /// <summary>
        /// Font
        /// 字体
        /// </summary>
        public string? Font { get; set; }

        /// <summary>
        /// Font size
        /// 字体大小
        /// </summary>
        public float? FontSize { get; set; }

        /// <summary>
        /// Font style
        /// 字体样式
        /// </summary>
        public PdfFontStyle? FontStyle { get; set; }

        /// <summary>
        /// Height
        /// 高度
        /// </summary>
        public float? Height { get; set; }

        /// <summary>
        /// Positioned element's left distance
        /// 定位元素的左边距离
        /// </summary>
        public float? Left { get; set; }

        /// <summary>
        /// Letter spacing
        /// 字母间距
        /// </summary>
        public float? LetterSpacing { get; set; }

        /// <summary>
        /// Line height
        /// 行高
        /// </summary>
        public float? LineHeight { get; set; }

        /// <summary>
        /// Margin
        /// 外延距离
        /// </summary>
        public PdfStyleSpace? Margin { get; set; }

        /// <summary>
        /// Padding
        /// 填充距离
        /// </summary>
        public PdfStyleSpace? Padding { get; set; }

        /// <summary>
        /// Position
        /// 位置
        /// </summary>
        public string? Position { get; set; }

        /// <summary>
        /// Text align
        /// 文本对齐
        /// </summary>
        public PdfTextAlign? TextAlign { get; set; }

        /// <summary>
        /// Text decoration
        /// 文字修饰
        /// </summary>
        public PdfTextDecoration? TextDecoration { get; set; }

        /// <summary>
        /// Text style
        /// 文本样式
        /// </summary>
        public PdfTextStyle? TextStyle { get; set; }


        /// <summary>
        /// Positioned element's top distance
        /// 定位元素的顶端距离
        /// </summary>
        public float? Top { get; set; }

        /// <summary>
        /// Width
        /// 宽度
        /// </summary>
        public float? Width { get; set; }

        /// <summary>
        /// Word spacing
        /// 字间距
        /// </summary>
        public float? WordSpacing { get; set; }

        private T? CalculatePropertyValue<T>(Func<PdfStyle, T> propertySelector)
        {
            PdfStyle? currentStyle = this;
            while (currentStyle != null)
            {
                var value = propertySelector(currentStyle);
                if (value != null)
                {
                    return value;
                }
                currentStyle = currentStyle.Parent;
            }
            return default;
        }

        public PdfStyle GetComputedStyle()
        {
            return new PdfStyle()
            {
                // Inherit
                Color = CalculatePropertyValue((style) => style.Color),
                Font = CalculatePropertyValue((style) => style.Font),
                FontSize = CalculatePropertyValue((style) => style.FontSize),
                FontStyle = CalculatePropertyValue((style) => style.FontStyle),
                LetterSpacing = CalculatePropertyValue((style) => style.LetterSpacing),
                LineHeight = CalculatePropertyValue((style) => style.LineHeight),
                TextAlign = CalculatePropertyValue((style) => style.TextAlign),
                TextDecoration = CalculatePropertyValue((style) => style.TextDecoration),
                TextStyle = CalculatePropertyValue((style) => style.TextStyle),
                WordSpacing = CalculatePropertyValue((style) => style.WordSpacing),

                // Not inherit
                BackgroundColor = BackgroundColor,
                Border = Border,
                Height = Height,
                Left = Left,
                Margin = Margin,
                Padding = Padding,
                Position = Position,
                Top = Top,
                Width = Width
            };
        }

        /// <summary>
        /// Get line height
        /// 获取行高
        /// </summary>
        /// <param name="fontLineHeight">Font default line height in pt</param>
        /// <returns>Result</returns>
        public float GetLineHeight(float fontLineHeight)
        {
            return LineHeight?.PxToPt() ?? fontLineHeight;
        }

        /// <summary>
        /// Get rectangle
        /// 获取矩形
        /// </summary>
        /// <param name="size">Default size</param>
        /// <param name="point">Start point inside the rectangle</param>
        /// <returns>Result</returns>
        public RectangleF GetRectangle(SizeF? size = null, Vector2? point = null)
        {
            var width = Width?.PxToPt() ?? size?.Width ?? 0;
            var height = Height?.PxToPt() ?? size?.Height ?? 0;
            var marginLeft = Margin?.Left.PxToPt() ?? 0;
            var marginTop = Margin?.Top.PxToPt() ?? 0;

            if (point.HasValue)
            {
                var p = point.Value;
                marginLeft += p.X;
                marginTop += p.Y;
                width -= p.X;
                height -= p.Y;
            }

            if (Position?.Equals(PositonAbsolte, StringComparison.OrdinalIgnoreCase) is true
                || Position?.Equals(PositonRelative, StringComparison.OrdinalIgnoreCase) is true)
            {
                var left = Left.GetValueOrDefault() + marginLeft;
                var top = Top.GetValueOrDefault() + marginTop;

                return new RectangleF(left, top, width, height);
            }
            else
            {
                return new RectangleF(marginLeft, marginTop, width, height);
            }
        }
    }
}