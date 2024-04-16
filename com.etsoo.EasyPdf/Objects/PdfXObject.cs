namespace com.etsoo.EasyPdf.Objects
{
    /// <summary>
    /// PDF external object (XObject)
    /// PDF 外部对象
    /// </summary>
    internal class PdfXObject : PdfStreamDic
    {
        public override string Type => "XObject";

        public readonly string SubType;

        public PdfXObject(string subType, ReadOnlyMemory<byte> bytes) : base(bytes)
        {
            SubType = subType;
        }

        protected override void AddItems()
        {
            base.AddItems();

            Dic.AddNames(nameof(SubType), SubType);
        }
    }
}
