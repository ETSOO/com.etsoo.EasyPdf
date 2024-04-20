namespace com.etsoo.EasyPdf.Objects
{
    /// <summary>
    /// PDF font stream
    /// PDF 字体流
    /// </summary>
    internal class PdfFontStream : PdfStreamDic
    {
        public string? Subtype { get; init; }

        public PdfFontStream(ReadOnlyMemory<byte> bytes) : base(bytes)
        {
        }

        protected override async Task AddItemsAsync()
        {
            await base.AddItemsAsync();

            Dic.AddNames(nameof(Subtype), Subtype);
        }
    }
}
