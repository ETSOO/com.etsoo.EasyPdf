using com.etsoo.EasyPdf.Types;
using System.Diagnostics.CodeAnalysis;

namespace com.etsoo.EasyPdf.Objects
{
    /// <summary>
    /// PDF name trees
    /// PDF 名称树
    /// </summary>
    /// <typeparam name="T">Generic name value type</typeparam>
    public record PdfNameTrees<T> where T : IPdfType
    {
        public PdfObject[]? Kids { get; }

        public SortedDictionary<string, T>? Names { get; }

        [NotNullIfNotNull(nameof(Names))]
        public (string, string)? Limits { get; }
    }
}
