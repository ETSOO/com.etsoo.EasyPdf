using System.Numerics;

namespace com.etsoo.EasyPdf.Support
{
    /// <summary>
    /// PDF point
    /// PDF 点
    /// </summary>
    public record PdfPoint
    {
        public float X { get; set; }
        public float Y { get; set; }

        public Vector2 ToVector2() => new(X, Y);
    }
}
