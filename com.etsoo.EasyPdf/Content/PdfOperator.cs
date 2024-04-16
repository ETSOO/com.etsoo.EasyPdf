using com.etsoo.HtmlUtils;
using System.Drawing;
using System.Numerics;
using System.Text;

namespace com.etsoo.EasyPdf.Content
{
    /// <summary>
    /// PDF operators
    /// https://www.syncfusion.com/succinctly-free-ebooks/pdf/text-operators
    /// A.1 Operator Summary
    /// PDF 运算符
    /// </summary>
    internal static class PdfOperator
    {
        // 9.3.6 Text Rendering Mode
        // 9.2.3 Achieving Special Graphical Effects
        // Q to exit clipping
        public enum TrMode : byte
        {
            Fill = 0,
            Stroke = 1,
            FillThenStroke = 2,
            Invisible = 3,
            FillThenClipping = 4,
            StrokeThenClipping = 5,
            FillThenStrokeThenClipping = 6,
            Clipping = 7
        }

        /// <summary>
        /// Close and stroke the path with a straight line segment
        /// </summary>
        public readonly static byte[] B = [66, PdfConstants.LineFeedByte];

        /// <summary>
        /// Close and stroke the path, possibly adding a bezier curve segment if necessary
        /// </summary>
        public readonly static byte[] b = [98, PdfConstants.LineFeedByte];

        /// <summary>
        /// Reset dash settings
        /// "[] 0 d\n"
        /// </summary>
        public readonly static byte[] d = [91, 93, PdfConstants.SpaceByte, 48, PdfConstants.SpaceByte, 100, PdfConstants.LineFeedByte];

        /// <summary>
        /// Begin a text object
        /// "BT\n"
        /// </summary>
        public readonly static byte[] BT = [66, 84, PdfConstants.LineFeedByte];

        /// <summary>
        /// End a text object
        /// "ET\n"
        /// </summary>
        public readonly static byte[] ET = [69, 84, PdfConstants.LineFeedByte];

        /// <summary>
        /// Close subpath
        /// </summary>
        public readonly static byte[] h = [104, PdfConstants.LineFeedByte];

        /// <summary>
        /// Show a text string
        /// " Tj\n"
        /// </summary>
        public readonly static byte[] Tj = [PdfConstants.SpaceByte, 84, 106, PdfConstants.LineFeedByte];

        /// <summary>
        /// Show a text string
        /// " TJ\n"
        /// </summary>
        public readonly static byte[] TJ = [PdfConstants.SpaceByte, 84, 74, PdfConstants.LineFeedByte];

        /// <summary>
        /// Text lending
        /// " TL\n"
        /// </summary>
        public readonly static byte[] TLBytes = [PdfConstants.SpaceByte, 84, 76, PdfConstants.LineFeedByte];

        /// <summary>
        /// T* Move to the start of the next line
        /// "T*\n"
        /// </summary>
        public readonly static byte[] T42 = [84, 42, PdfConstants.LineFeedByte];

        /// <summary>
        /// Close and strokes the path
        /// "s\n"
        /// </summary>
        public readonly static byte[] s = [115, PdfConstants.LineFeedByte];

        /// <summary>
        /// Strokes the path defined by the preceding drawing commands
        /// "S\n"
        /// </summary>
        public readonly static byte[] S = [83, PdfConstants.LineFeedByte];

        /// <summary>
        /// Single quote
        /// moves to the next line then displays the text. This is the exact same functionality as T* followed by Tj
        /// "'\n"
        /// </summary>
        public readonly static byte[] SQ = [39, PdfConstants.LineFeedByte];

        /// <summary>
        /// Save graphics state
        /// "q\n"
        /// </summary>
        public readonly static byte[] q = [113, PdfConstants.LineFeedByte];

        /// <summary>
        /// Restore graphics state
        /// "Q\n"
        /// </summary>
        public readonly static byte[] Q = [81, PdfConstants.LineFeedByte];

        /// <summary>
        /// Rotate the coordinate system, clockwise
        /// </summary>
        /// <param name="angle">Angle</param>
        /// <param name="x">Position X</param>
        /// <param name="y">Position Y</param>
        /// <returns>Bytes</returns>
        public static byte[] Rotate(float angle, float x, float y)
        {
            var cosAngle = Math.Cos(angle).ToSingle();
            var sinAngle = Math.Sin(angle).ToSingle();
            return Tm(cosAngle, -sinAngle, sinAngle, cosAngle, x, y, true);
        }

        /// <summary>
        /// Set RGB color for stroking operations
        /// </summary>
        /// <param name="color">Color</param>
        /// <returns>Bytes</returns>
        public static byte[] RG(PdfColor? color)
        {
            return
            [
                .. Encoding.ASCII.GetBytes(color.HasValue ? $"{color}" : "0 0 0"),
                .. " RG\n"u8
            ];
        }

        /// <summary>
        /// Set RGB color for stroking and nonstroking operations
        /// </summary>
        /// <param name="color">Color</param>
        /// <returns>Bytes</returns>
        public static byte[] RG2(PdfColor? color)
        {
            var bytes = RG(color);
            return [.. bytes, .. Zrg(color)];
        }

        /// <summary>
        /// Set RGB color for nonstroking (filling) operations
        /// </summary>
        /// <param name="color">Color</param>
        /// <returns>Bytes</returns>
        public static byte[] Zrg(PdfColor? color)
        {
            return
            [
                .. Encoding.ASCII.GetBytes(color.HasValue ? $"{color}" : "0 0 0"),
                .. " rg\n"u8
            ];
        }

        /// <summary>
        /// Letter spacing
        /// </summary>
        /// <param name="spacing">Spacing</param>
        /// <returns>Bytes</returns>
        public static byte[] Tc(float spacing)
        {
            return
            [
                .. Encoding.ASCII.GetBytes(spacing.ToString()),
                .. " Tc\n"u8
            ];
        }

        /// <summary>
        /// Move to the start of the next line, offset from the start of the current line by (tx , ty )
        /// </summary>
        /// <param name="point">Point</param>
        /// <returns>Bytes</returns>
        public static byte[] Td(Vector2 point)
        {
            return Td(point.X, point.Y);
        }

        public static byte[] Td(float x, float y)
        {
            return
            [
                .. Encoding.ASCII.GetBytes($"{x} {y}"),
                .. " Td\n"u8
            ];
        }

        /// <summary>
        /// Set text font and size
        /// </summary>
        /// <returns>Bytes</returns>
        public static byte[] Tf(string font, float size)
        {
            return
            [
                .. Encoding.ASCII.GetBytes($"/{font} {size}"),
                .. " Tf\n"u8
            ];
        }

        /// <summary>
        /// Set the text leading
        /// </summary>
        /// <param name="leading">Leading</param>
        /// <returns>Bytes</returns>
        public static byte[] TL(float leading)
        {
            return
            [
                .. Encoding.ASCII.GetBytes($"{leading}"),
                .. TLBytes
            ];
        }

        /// <summary>
        /// Set the text matrix
        /// Most of the other text positioning and text state commands are simply predefined operations on the transformation matrix
        /// 
        /// a b 0
        /// c d 0
        /// e f 1
        /// 
        /// The a and d values determine its horizontal and vertical scale (obtained by [ sx 0 0 sy 0 0 ])
        /// The e and f values determine the horizontal and vertical position of the text
        /// Rotations are produced by [ cos θ sin θ −sin θ cos θ 0 0 ]
        ///     which has the effect of rotating the coordinate system axes by an angle θ counterclockwise
        /// Skew is specified by [ 1 tan α tan β 1 0 0 ], which skews the x axis by an angle α and the y axis by an angle β
        /// cm manipulates the current transformation matrix (CTM), an element of the PDF graphics state, which defines the transformation from user space to device space
        /// Tm will inherit the current transformation matrix within the BT and ET operators
        /// https://stackoverflow.com/questions/34900352/pdf-image-positioning
        /// </summary>
        /// <returns>Bytes</returns>
        public static byte[] Tm(float a, float b, float c, float d, float e, float f, bool cm = false)
        {
            // cm - Concatenate matrix to current transformation matrix
            return
            [
                .. Encoding.ASCII.GetBytes($"{a} {b} {c} {d} {e} {f}"),
                .. new byte[]
                {
                    PdfConstants.SpaceByte,
                    (byte)(cm ? 99 : 84), // c or T
                    109, // m
                    PdfConstants.LineFeedByte
                }
            ];
        }

        /// <summary>
        /// Text rendering mode
        /// </summary>
        /// <param name="mode">Mode</param>
        /// <returns>Bytes</returns>
        public static byte[] Tr(TrMode mode)
        {
            return
            [
                (byte)(48 + (byte)mode),
                PdfConstants.SpaceByte,
                84,
                114,
                PdfConstants.LineFeedByte
            ];
        }

        /// <summary>
        /// Offsets the vertical position of the text to create superscripts or subscripts
        /// </summary>
        /// <param name="offset">Offset</param>
        /// <returns>Bytes</returns>
        public static byte[] Ts(float offset)
        {
            return
            [
                .. Encoding.ASCII.GetBytes($"{offset}"),
                .. " Ts\n"u8
            ];
        }

        /// <summary>
        /// Word spacing affects the space between words
        /// only works with single-byte character codes (simple font)
        /// </summary>
        /// <param name="spacing">Spacing</param>
        /// <returns>Bytes</returns>
        public static byte[] Tw(float spacing)
        {
            return
            [
                .. Encoding.ASCII.GetBytes(spacing.ToString()),
                .. " Tw\n"u8
            ];
        }

        /// <summary>
        /// Line to the point
        /// </summary>
        /// <param name="point">Point</param>
        /// <returns>Bytes</returns>
        public static byte[] Zl(Vector2 point)
        {
            return
            [
                .. Encoding.ASCII.GetBytes($"{point.X} {point.Y}"),
                .. " l\n"u8
            ];
        }

        /// <summary>
        /// Defines the dash pattern, where 'width' represents the length of the dash and 'height' represents the length of the gap
        /// </summary>
        /// <param name="size">Size, null to reset</param>
        /// <returns>Bytes</returns>
        public static byte[] Zd(HtmlSize? size = null)
        {
            if (size.HasValue)
            {
                return Zd(size.Value);
            }
            else
            {
                return d;
            }
        }

        /// <summary>
        /// Defines the dash pattern, where 'width' represents the length of the dash and 'height' represents the length of the gap
        /// </summary>
        /// <param name="size">Size, null to reset</param>
        /// <returns>Bytes</returns>
        public static byte[] Zd<T>(T width, T height) where T : INumber<T>
        {
            return
            [
                .. Encoding.ASCII.GetBytes($"[{width} {height}]"),
                    .. " 0 d\n"u8
            ];
        }

        /// <summary>
        /// Apply a graphics state parameter dictionary
        /// 使用图形状态参数字典
        /// </summary>
        /// <param name="refName">Reference name</param>
        /// <returns>Bytes</returns>
        public static byte[] Zgs(string refName)
        {
            return
            [
                .. Encoding.ASCII.GetBytes($"/{refName}"),
                .. " gs\n"u8
            ];
        }

        /// <summary>
        /// Moves the current point to the position (point) without drawing anything
        /// </summary>
        /// <param name="point">Point</param>
        /// <returns>Bytes</returns>
        public static byte[] Zm(Vector2 point)
        {
            return
            [
                .. Encoding.ASCII.GetBytes($"{point.X} {point.Y}"),
                .. " m\n"u8
            ];
        }

        /// <summary>
        /// Define a rectangle from lower-left corner
        /// </summary>
        /// <param name="rect">Rectangle</param>
        /// <returns>Bytes</returns>
        public static byte[] Zre(Rectangle rect)
        {
            return
            [
                .. Encoding.ASCII.GetBytes($"{rect.X} {rect.Y} {rect.Width} {rect.Height}"),
                .. " re\n"u8
            ];
        }

        public static byte[] Zre(RectangleF rect)
        {
            return
            [
                .. Encoding.ASCII.GetBytes($"{rect.X} {rect.Y} {rect.Width} {rect.Height}"),
                .. " re\n"u8
            ];
        }

        /// <summary>
        /// Set line width in pixels
        /// </summary>
        /// <param name="width">Width</param>
        /// <returns>Bytes</returns>
        public static byte[] Zw(float width)
        {
            return
            [
                .. Encoding.ASCII.GetBytes($"{width}"),
                .. " w\n"u8
            ];
        }
    }
}
