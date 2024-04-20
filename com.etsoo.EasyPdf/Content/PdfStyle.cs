using com.etsoo.EasyPdf.Support;
using System.Drawing;

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
        /// Auto inherit
        /// 自动继承
        /// </summary>
        public bool Inherit { get; set; } = true;

        /// <summary>
        /// Background color
        /// 背景颜色
        /// </summary>
        public PdfColor? BackgroundColor { get; set; }

        /// <summary>
        /// Set background color
        /// 设置背景颜色
        /// </summary>
        /// <param name="backgroundColor">Background color</param>
        /// <returns>Style</returns>
        public PdfStyle SetBackgroundColor(PdfColor? backgroundColor)
        {
            BackgroundColor = backgroundColor;
            return this;
        }

        /// <summary>
        /// Border
        /// 边框
        /// </summary>
        public PdfStyleBorder? Border { get; set; }

        /// <summary>
        /// Set border
        /// 设置边框
        /// </summary>
        /// <param name="border">Border</param>
        /// <returns>Style</returns>
        public PdfStyle SetBorder(PdfStyleBorder? border)
        {
            Border = border;
            return this;
        }

        /// <summary>
        /// Color
        /// 颜色
        /// </summary>
        public PdfColor? Color { get; set; }

        /// <summary>
        /// Set color
        /// 设置颜色
        /// </summary>
        /// <param name="color">Color</param>
        /// <returns>Style</returns>
        public PdfStyle SetColor(PdfColor? color)
        {
            Color = color;
            return this;
        }

        /// <summary>
        /// Font
        /// 字体
        /// </summary>
        public string? Font { get; set; }

        /// <summary>
        /// Set font
        /// 设置字体
        /// </summary>
        /// <param name="font">Font</param>
        /// <returns>Style</returns>
        public PdfStyle SetFont(string? font)
        {
            Font = font;
            return this;
        }

        /// <summary>
        /// Font size
        /// 字体大小
        /// </summary>
        public float? FontSize { get; set; }

        /// <summary>
        /// Set font size
        /// 设置字体大小
        /// </summary>
        /// <param name="fontSize">Font size</param>
        /// <returns>Style</returns>
        public PdfStyle SetFontSize(float? fontSize)
        {
            FontSize = fontSize;
            return this;
        }

        /// <summary>
        /// Font style
        /// 字体样式
        /// </summary>
        public PdfFontStyle? FontStyle { get; set; }

        /// <summary>
        /// Set font style
        /// 设置字体样式
        /// </summary>
        /// <param name="fontStyle">Font style</param>
        /// <returns>Style</returns>
        public PdfStyle SetFontStyle(PdfFontStyle? fontStyle)
        {
            FontStyle = fontStyle;
            return this;
        }

        /// <summary>
        /// Height
        /// 高度
        /// </summary>
        public float? Height { get; set; }

        /// <summary>
        /// Set height
        /// 设置高度
        /// </summary>
        /// <param name="height">Height</param>
        /// <returns>Style</returns>
        public PdfStyle SetHeight(float? height)
        {
            Height = height;
            return this;
        }

        /// <summary>
        /// Positioned element's left distance
        /// 定位元素的左边距离
        /// </summary>
        public float? Left { get; set; }

        /// <summary>
        /// Set left distance
        /// 设置左边距离
        /// </summary>
        /// <param name="left">Left distance</param>
        /// <returns>Style</returns>
        public PdfStyle SetLeft(float? left)
        {
            Left = left;
            return this;
        }

        /// <summary>
        /// Letter spacing
        /// 字母间距
        /// </summary>
        public float? LetterSpacing { get; set; }

        /// <summary>
        /// Set letter spacing
        /// 设置字母间距
        /// </summary>
        /// <param name="letterSpacing">Letter spacing</param>
        /// <returns>Style</returns>
        public PdfStyle SetLetterSpacing(float? letterSpacing)
        {
            LetterSpacing = letterSpacing;
            return this;
        }

        /// <summary>
        /// Line height
        /// 行高
        /// </summary>
        public float? LineHeight { get; set; }

        /// <summary>
        /// Set line height
        /// 设置行高
        /// </summary>
        /// <param name="lineHeight">Line height</param>
        /// <returns>Style</returns>
        public PdfStyle SetLineHeight(float? lineHeight)
        {
            LineHeight = lineHeight;
            return this;
        }

        /// <summary>
        /// Margin
        /// 外延距离
        /// </summary>
        public PdfStyleSpace Margin { get; set; } = new PdfStyleSpace(0);

        /// <summary>
        /// Set margin
        /// 设置外延距离
        /// </summary>
        /// <param name="margin">Margin</param>
        /// <returns>Style</returns>
        public PdfStyle SetMargin(PdfStyleSpace margin)
        {
            Margin = margin;
            return this;
        }

        /// <summary>
        /// Padding
        /// 填充距离
        /// </summary>
        public PdfStyleSpace Padding { get; set; } = new PdfStyleSpace(0);

        /// <summary>
        /// Set padding
        /// 设置填充距离
        /// </summary>
        /// <param name="padding">Padding</param>
        /// <returns>Style</returns>
        public PdfStyle SetPadding(PdfStyleSpace padding)
        {
            Padding = padding;
            return this;
        }

        /// <summary>
        /// Position
        /// 位置
        /// </summary>
        public string? Position { get; set; }

        /// <summary>
        /// Set position
        /// 设置位置
        /// </summary>
        /// <param name="position">Position</param>
        /// <returns>Style</returns>
        public PdfStyle SetPosition(string? position)
        {
            Position = position;
            return this;
        }

        /// <summary>
        /// Opacity, 0 - 1 number
        /// 不透明度
        /// </summary>
        public float Opacity { get; set; } = 1;

        /// <summary>
        /// Set opacity
        /// 设置不透明度
        /// </summary>
        /// <param name="opacity">Opacity, 0 - 1 number</param>
        /// <returns>Style</returns>
        public PdfStyle SetOpacity(float opacity)
        {
            Opacity = opacity;
            return this;
        }

        /// <summary>
        /// Rotate angle
        /// 旋转角度
        /// </summary>
        public float Rotate { get; set; }

        /// <summary>
        /// Set rotate angle
        /// 设置旋转角度
        /// </summary>
        /// <param name="angle">Rotate angle</param>
        /// <returns>Style</returns>
        public PdfStyle SetRotate(float angle)
        {
            if (Math.Abs(angle) > 6 && angle == (int)angle)
            {
                angle = Convert.ToSingle(angle * Math.PI / 180);
            }

            Rotate = angle;
            return this;
        }

        /// <summary>
        /// Text align
        /// 文本对齐
        /// </summary>
        public PdfTextAlign? TextAlign { get; set; }

        /// <summary>
        /// Set text align
        /// 设置文本对齐
        /// </summary>
        /// <param name="textAlign">Text align</param>
        /// <returns>Style</returns>
        public PdfStyle SetTextAlign(PdfTextAlign? textAlign)
        {
            TextAlign = textAlign;
            return this;
        }

        /// <summary>
        /// Text decoration
        /// 文字修饰
        /// </summary>
        public PdfTextDecoration? TextDecoration { get; set; }

        /// <summary>
        /// Set text decoration
        /// 设置文字修饰
        /// </summary>
        /// <param name="textDecoration">Text decoration</param>
        /// <returns>Style</returns>
        public PdfStyle SetTextDecoration(PdfTextDecoration? textDecoration)
        {
            TextDecoration = textDecoration;
            return this;
        }

        /// <summary>
        /// Positioned element's top distance
        /// 定位元素的顶端距离
        /// </summary>
        public float? Top { get; set; }

        /// <summary>
        /// Set top
        /// 设置顶端距离
        /// </summary>
        /// <param name="top">Top</param>
        /// <returns>Style</returns>
        public PdfStyle SetTop(float? top)
        {
            Top = top;
            return this;
        }

        /// <summary>
        /// Width
        /// 宽度
        /// </summary>
        public float? Width { get; set; }

        /// <summary>
        /// Set width
        /// 设置宽度
        /// </summary>
        /// <param name="width">Width</param>
        /// <returns>Style</returns>
        public PdfStyle SetWidth(float? width)
        {
            Width = width;
            return this;
        }

        /// <summary>
        /// Word spacing
        /// 字间距
        /// </summary>
        public float? WordSpacing { get; set; }

        /// <summary>
        /// Set word spacing
        /// 设置字间距
        /// </summary>
        /// <param name="wordSpacing">Word spacing</param>
        /// <returns>Style</returns>
        public PdfStyle SetWordSpacing(float? wordSpacing)
        {
            WordSpacing = wordSpacing;
            return this;
        }

        private T? CalculatePropertyValue<T>(Func<PdfStyle, T> propertySelector, bool inherit)
        {
            PdfStyle? currentStyle = this;

            if (!inherit)
            {
                return propertySelector(currentStyle);
            }

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

        /// <summary>
        /// Get computed style
        /// 获取计算后的样式
        /// </summary>
        /// <returns>Style</returns>
        public PdfStyle GetComputedStyle()
        {
            return new PdfStyle()
            {
                // Inherit
                Color = CalculatePropertyValue((style) => style.Color, true),
                Font = CalculatePropertyValue((style) => style.Font, true),
                FontStyle = CalculatePropertyValue((style) => style.FontStyle, true),

                FontSize = CalculatePropertyValue((style) => style.FontSize, Inherit),
                LetterSpacing = CalculatePropertyValue((style) => style.LetterSpacing, Inherit),
                LineHeight = CalculatePropertyValue((style) => style.LineHeight, Inherit),
                TextAlign = CalculatePropertyValue((style) => style.TextAlign, Inherit),
                TextDecoration = CalculatePropertyValue((style) => style.TextDecoration, Inherit),
                WordSpacing = CalculatePropertyValue((style) => style.WordSpacing, Inherit),

                // Not inherit
                BackgroundColor = BackgroundColor,
                Border = Border,
                Height = Height,
                Left = Left,
                Margin = Margin,
                Opacity = Opacity,
                Padding = Padding,
                Position = Position,
                Rotate = Rotate,
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
        public RectangleF GetRectangle(SizeF? size = null, PdfPoint? point = null)
        {
            var width = Width?.PxToPt() ?? size?.Width ?? 0;
            var height = Height?.PxToPt() ?? size?.Height ?? 0;

            var marginLeft = Margin?.Left.PxToPt() ?? 0;
            var marginTop = Margin?.Top.PxToPt() ?? 0;

            var marginRight = Margin?.Right.PxToPt() ?? 0;
            var marginBottom = Margin?.Bottom.PxToPt() ?? 0;

            width -= marginLeft + marginRight;
            height -= marginTop + marginBottom;

            float left, top;

            if (Position?.Equals(PositonAbsolte, StringComparison.OrdinalIgnoreCase) is true
                || Position?.Equals(PositonRelative, StringComparison.OrdinalIgnoreCase) is true)
            {
                left = Left.GetValueOrDefault() + marginLeft;
                top = Top.GetValueOrDefault() + marginTop;
            }
            else
            {
                if (point != null)
                {
                    left = point.X;
                    top = point.Y;
                    width -= point.X;
                    height -= point.Y;

                    // Update position
                    point.X += marginLeft;
                    point.Y += marginTop;
                }
                else
                {
                    left = 0;
                    top = 0;
                }

                left += marginLeft;
                top += marginTop;
            }

            return new RectangleF(left, top, width, height);
        }

        public (int width, int height) GetSize(float sourceWidth, float sourceHeight, System.Drawing.RectangleF rect)
        {
            var cssWidth = Width;
            var cssHeight = Height;

            int width, height;

            var ratio = sourceWidth / sourceHeight;
            if (cssWidth.HasValue)
            {
                width = (int)cssWidth.Value.PxToPt();

                if (cssHeight.HasValue)
                {
                    height = (int)cssHeight.Value.PxToPt();
                }
                else
                {
                    height = (int)(width / ratio);
                }
            }
            else
            {
                if (cssHeight.HasValue)
                {
                    height = (int)cssHeight.Value.PxToPt();
                    width = (int)(height * ratio);
                }
                else
                {
                    width = (int)rect.Width;
                    height = (int)rect.Height;
                }
            }

            return (width, height);
        }
    }
}