using com.etsoo.EasyPdf.Types;
using System.Diagnostics.CodeAnalysis;

namespace com.etsoo.EasyPdf.Objects
{
    /// <summary>
    /// PDF number trees
    /// PDF 数字树
    /// </summary>
    /// <typeparam name="T">Generic number value type</typeparam>
    public record PdfNumberTrees<T> where T : IPdfType
    {
        public PdfObject[]? Kids { get; }

        public SortedDictionary<int, T>? Nums { get; }

        [NotNullIfNotNull(nameof(Nums))]
        public (int, int)? Limits { get; }
    }
}
