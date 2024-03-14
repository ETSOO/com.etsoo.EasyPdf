using com.etsoo.EasyPdf.Types;
using System.Diagnostics.CodeAnalysis;

namespace com.etsoo.EasyPdf.Objects
{
    public record PdfNameTrees<T> where T : IPdfType
    {
        public PdfObject[]? Kids { get; }

        public SortedDictionary<string, T>? Names { get; }

        [NotNullIfNotNull(nameof(Names))]
        public (string, string)? Limits { get; }
    }
}
