using com.etsoo.EasyPdf.Types;

namespace com.etsoo.EasyPdf.Objects
{
    public class PdfFontStream : PdfStreamDic
    {
        public string? Subtype { get; init; }

        public int[]? Lengths { get; init; }

        public PdfFontStream(ReadOnlyMemory<byte> bytes, PdfDictionary? dic = null) : base(bytes, dic)
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
