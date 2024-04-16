namespace com.etsoo.EasyPdf.Objects
{
    /// <summary>
    /// PDF font stream
    /// PDF 字体流
    /// </summary>
    public class PdfFontStream : PdfStreamDic
    {
        public string? Subtype { get; init; }

        public PdfFontStream(ReadOnlyMemory<byte> bytes) : base(bytes)
        {
        }

        protected override void AddItems()
        {
            base.AddItems();

            Dic.AddNames(nameof(Subtype), Subtype);
        }
    }
}
