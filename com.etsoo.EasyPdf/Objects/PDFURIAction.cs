namespace com.etsoo.EasyPdf.Objects
{
    /// <summary>
    /// PDF URI action
    /// Table 206 – Additional entries specific to a URI action
    /// PDF URI 动作
    /// </summary>
    internal class PDFURIAction : PdfAction
    {
        public string URI { get; }

        public bool IsMap { get; set; }

        public PDFURIAction(string uri) : base("URI")
        {
            URI = uri;
        }

        protected override async Task AddItemsAsync()
        {
            await base.AddItemsAsync();

            Dic.AddNameStr(nameof(URI), URI);
            Dic.AddNameBool(nameof(IsMap), IsMap);
        }
    }
}
