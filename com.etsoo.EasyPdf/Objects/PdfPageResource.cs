using com.etsoo.EasyPdf.Types;

namespace com.etsoo.EasyPdf.Objects
{
    /// <summary>
    /// Procedures
    /// Beginning with PDF 1.4, this feature is considered obsolete
    /// </summary>
    public enum PdfPageResourceProcedure
    {
        PDF,
        Text,
        ImageB,
        ImageC,
        ImageI
    }

    /// <summary>
    /// PDF page resource
    /// PDF 页面资源
    /// </summary>
    internal record PdfPageResource : PdfDictionary
    {
        public List<PdfPageResourceProcedure> ProcSet { get; } = [PdfPageResourceProcedure.PDF];

        public Dictionary<string, PdfObject> ExtGState { get; } = [];

        public Dictionary<string, PdfObject> Font { get; } = [];

        public Dictionary<string, PdfObject> XObject { get; } = [];

        public override Task WriteToAsync(Stream stream)
        {
            if (ExtGState.Count != 0)
            {
                AddNameDic(nameof(ExtGState), ExtGState);
            }

            if (Font.Count != 0)
            {
                ProcSet.Add(PdfPageResourceProcedure.Text);
                AddNameDic(nameof(Font), Font);
            }

            if (XObject.Count != 0)
            {
                ProcSet.AddRange([PdfPageResourceProcedure.ImageB, PdfPageResourceProcedure.ImageC, PdfPageResourceProcedure.ImageI]);
                AddNameDic(nameof(XObject), XObject);
            }

            AddNameArray(nameof(ProcSet), ProcSet.Select(p => new PdfName(p.ToString())));

            return base.WriteToAsync(stream);
        }
    }
}
