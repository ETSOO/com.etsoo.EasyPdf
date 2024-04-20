namespace com.etsoo.EasyPdf.Objects
{
    /// <summary>
    /// PDF graphics state parameter dictionary
    /// PDF 图形状态参数字典
    /// </summary>
    internal class PdfExtGState : PdfObjectDic
    {
        public float CA { get; set; }
        public float ca { get; set; }

        protected override async Task AddItemsAsync()
        {
            await base.AddItemsAsync();

            Dic.AddNameNum(nameof(CA), CA);
            Dic.AddNameNum(nameof(ca), ca);
        }
    }
}
