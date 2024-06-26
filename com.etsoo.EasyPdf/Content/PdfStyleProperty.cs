﻿namespace com.etsoo.EasyPdf.Content
{
    /// <summary>
    /// PDF position style
    /// https://developer.mozilla.org/en-US/docs/Web/CSS/position
    /// PDF 位置样式
    /// </summary>
    public enum PdfPosition
    {
        Static,
        Relative,
        Absolute
    }

    /// <summary>
    /// PDF line style
    /// PDF 线条样式
    /// </summary>
    public enum PdfLineStyle
    {
        Solid,
        Dotted
    }

    /// <summary>
    /// PDF line kind
    /// PDf 线条类型
    /// </summary>
    [Flags]
    public enum PdfLineKind
    {
        Underline = 1,
        LineThrough = 2
    }

    /// <summary>
    /// PDF text decoration
    /// PDF 文字修饰
    /// </summary>
    public record PdfTextDecoration
    {
        /// <summary>
        /// Kind
        /// 类型
        /// </summary>
        public PdfLineKind Kind { get; set; }

        /// <summary>
        /// Line style
        /// 线条样式
        /// </summary>
        public PdfLineStyle Style { get; set; }

        /// <summary>
        /// Color
        /// 颜色
        /// </summary>
        public PdfColor? Color { get; set; }

        /// <summary>
        /// Thickness
        /// 厚度
        /// </summary>
        public ushort Thickness { get; set; }

        /// <summary>
        /// Constructor
        /// 构造函数
        /// </summary>
        /// <param name="kind">Kind</param>
        /// <param name="color">Color</param>
        /// <param name="style">Line style</param>
        /// <param name="thickness">Thickness</param>
        public PdfTextDecoration(PdfLineKind kind, PdfColor color, PdfLineStyle style = PdfLineStyle.Solid, ushort thickness = 1)
        {
            Kind = kind;
            Style = style;
            Color = color;
            Thickness = thickness;
        }
    }

    /// <summary>
    /// The font styles
    /// 字体样式
    /// </summary>
    [Flags]
    public enum PdfFontStyle
    {
        /// <summary>
        /// Regular
        /// </summary>
        Regular = 0,

        /// <summary>
        /// Bold
        /// </summary>
        Bold = 1,

        /// <summary>
        /// Italic
        /// </summary>
        Italic = 2,

        /// <summary>
        /// Bold and Italic
        /// </summary>
        BoldItalic = 3
    }

    /// <summary>
    /// PDF style space definition in pt (like padding and margin)
    /// PDF 样式空间定义
    /// </summary>
    public record PdfStyleSpace
    {
        /// <summary>
        /// Zero space
        /// 零空间
        /// </summary>
        public static PdfStyleSpace Zero { get; } = new(0);

        public float Left { get; set; }
        public float Top { get; set; }
        public float Right { get; set; }
        public float Bottom { get; set; }

        public PdfStyleSpace(float left, float top, float right, float bottom)
        {
            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
        }

        public PdfStyleSpace(float space) : this(space, space, space, space)
        {
        }

        public PdfStyleSpace(float vertical, float horizontal) : this(horizontal, vertical, horizontal, vertical)
        {
        }
    }

    /// <summary>
    /// PDF text align
    /// https://developer.mozilla.org/en-US/docs/Web/CSS/text-align
    /// PDF 文字对齐
    /// </summary>
    public enum PdfTextAlign
    {
        Start,
        Center,
        End,
        Justify
    }

    /// <summary>
    /// Border style
    /// https://developer.mozilla.org/en-US/docs/Web/CSS/border-style
    /// 边框样式
    /// </summary>
    public enum PdfStyleBorderStyle
    {
        None,
        Dashed,
        Dotted,
        Solid
    }

    /// <summary>
    /// Border side style
    /// 边框边样式
    /// </summary>
    public record PdfStyleBorderSide
    {
        /// <summary>
        /// None border
        /// 空边框
        /// </summary>
        public static PdfStyleBorderSide None => new(PdfColor.Black, 0, PdfStyleBorderStyle.None);

        public virtual PdfColor Color { get; set; }

        public virtual float Width { get; set; }

        public virtual PdfStyleBorderStyle Style { get; set; }

        public virtual bool HasBorder => Style != PdfStyleBorderStyle.None && Width > 0;

        public PdfStyleBorderSide(PdfColor color, float width = 1, PdfStyleBorderStyle style = PdfStyleBorderStyle.Solid)
        {
            Color = color;
            Width = width;
            Style = style;
        }
    }

    /// <summary>
    /// Border style
    /// 边框样式
    /// </summary>
    public record PdfStyleBorder : PdfStyleBorderSide
    {
        override public PdfColor Color
        {
            get
            {
                return base.Color;
            }
            set
            {
                base.Color = value;

                if (Top == null) return;
                Top.Color = value;
                Right.Color = value;
                Bottom.Color = value;
                Left.Color = value;
            }
        }

        override public float Width
        {
            get
            {
                return base.Width;
            }
            set
            {
                base.Width = value;

                if (Top == null) return;
                Top.Width = value;
                Right.Width = value;
                Bottom.Width = value;
                Left.Width = value;
            }
        }

        override public PdfStyleBorderStyle Style
        {
            get
            {
                return base.Style;
            }
            set
            {
                base.Style = value;

                if (Top == null) return;
                Top.Style = value;
                Right.Style = value;
                Bottom.Style = value;
                Left.Style = value;
            }
        }

        public PdfStyleBorderSide Top { get; internal set; }

        public PdfStyleBorderSide Right { get; internal set; }

        public PdfStyleBorderSide Bottom { get; internal set; }

        public PdfStyleBorderSide Left { get; internal set; }

        public PdfStyleSpace? Radius { get; internal set; }

        public override bool HasBorder => Left.HasBorder || Top.HasBorder || Right.HasBorder || Bottom.HasBorder;

        public bool SameStyle => Left.Equals(Top) && Left.Equals(Right) && Left.Equals(Bottom);

        public PdfStyleBorder(PdfColor color, float width = 1, PdfStyleBorderStyle style = PdfStyleBorderStyle.Solid)
            : base(color, width, style)
        {
            Top = new PdfStyleBorderSide(color, width, style);
            Right = new PdfStyleBorderSide(color, width, style);
            Bottom = new PdfStyleBorderSide(color, width, style);
            Left = new PdfStyleBorderSide(color, width, style);
        }
    }
}
