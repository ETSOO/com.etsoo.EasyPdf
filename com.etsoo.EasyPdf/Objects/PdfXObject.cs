namespace com.etsoo.EasyPdf.Objects
{
    /// <summary>
    /// PDF external object (XObject)
    /// PDF 外部对象
    /// </summary>
    internal class PdfXObject : PdfStreamDic
    {
        public override string Type => "XObject";

        public readonly string Subtype;

        public PdfXObject(string subtype, ReadOnlyMemory<byte> bytes) : base(bytes)
        {
            Subtype = subtype;
        }

        protected override async Task AddItemsAsync()
        {
            await base.AddItemsAsync();

            Dic.AddNames(nameof(Subtype), Subtype);
        }
    }
}
