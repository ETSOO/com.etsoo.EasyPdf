namespace com.etsoo.EasyPdf.Objects
{
    /// <summary>
    /// PDF font stream
    /// PDF 字体流
    /// </summary>
    public class PdfFontStream : PdfStreamDic
    {
        public string? Subtype { get; init; }

        public int[]? Lengths { get; init; }

        public PdfFontStream(ReadOnlyMemory<byte> bytes) : base(bytes)
        {
        }

        protected override void AddItems()
        {
            base.AddItems();

            Dic.AddNames(nameof(Subtype), Subtype);

            if (Lengths != null)
            {
                for (var k = 0; k < Lengths.Length; k++)
                {
                    Dic.AddNameInt($"Length{k + 1}", Lengths[k]);
                }
            }
        }
    }
}
