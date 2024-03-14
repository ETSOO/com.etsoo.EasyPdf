using com.etsoo.EasyPdf.Types;
using System.Diagnostics.CodeAnalysis;

namespace com.etsoo.EasyPdf.Objects
{
    public record PdfNumberTrees<T> where T : IPdfType
    {
        public PdfObject[]? Kids { get; }

        public SortedDictionary<int, T>? Nums { get; }

        [NotNullIfNotNull(nameof(Nums))]
        public (int, int)? Limits { get; }
    }
}
