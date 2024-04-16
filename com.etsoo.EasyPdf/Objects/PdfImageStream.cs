using com.etsoo.EasyPdf.Types;

namespace com.etsoo.EasyPdf.Objects
{
    /// <summary>
    /// PDF image stream
    /// PDF 图片流
    /// Table 89 - Additional entries specific to an image dictionary
    /// </summary>
    internal class PdfImageStream : PdfXObject
    {
        public int BitsPerComponent { get; set; } = 8;
        public IPdfType? ColorSpace { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        public PdfImageStream(ReadOnlyMemory<byte> bytes, int width, int height) : base("Image", bytes)
        {
            Width = width;
            Height = height;
        }

        protected override void AddItems()
        {
            base.AddItems();

            Dic.AddNameInt(nameof(BitsPerComponent), BitsPerComponent);
            Dic.AddNameItem(nameof(ColorSpace), ColorSpace);
            Dic.AddNameInt(nameof(Width), Width);
            Dic.AddNameInt(nameof(Height), Height);
        }
    }
}
