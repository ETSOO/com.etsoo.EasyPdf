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

        public PdfImageStream? SMaskStream { get; set; }
        public PdfObject? SMask { get; set; }

        public PdfImageStream(ReadOnlyMemory<byte> bytes, int width, int height) : base("Image", bytes)
        {
            Width = width;
            Height = height;
        }

        protected override async Task AddItemsAsync()
        {
            await base.AddItemsAsync();

            Dic.AddNameInt(nameof(BitsPerComponent), BitsPerComponent);
            Dic.AddNameItem(nameof(ColorSpace), ColorSpace);
            Dic.AddNameInt(nameof(Width), Width);
            Dic.AddNameInt(nameof(Height), Height);
            Dic.AddNameItem(nameof(SMask), SMask);
        }

        public override async Task WriteRelatedStreams(PdfWriter writer, Stream stream)
        {
            // SMaskStream.SMask == null is to avoid infinite loop
            if (SMaskStream != null && SMaskStream.SMask == null && SMaskStream.Obj != null)
            {
                SMask = await writer.WriteDicAsync(SMaskStream);
            }
        }
    }
}
