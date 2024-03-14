namespace com.etsoo.EasyPdf.Filters
{
    internal class FilterParams
    {
    }

    internal class FlateFilterParams : FilterParams
    {
        /// <summary>
        /// 1 = default
        /// 2 = TIFF Predictor 2
        /// 10 = PNG prediction (on encoding, PNG None on all rows)
        /// 11 = PNG prediction (on encoding, PNG Sub on all rows)
        /// 12 = PNG prediction (on encoding, PNG Up on all rows)
        /// 13 = PNG prediction (on encoding, PNG Average on all rows)
        /// 14 = PNG prediction (on encoding, PNG Paeth on all rows)
        /// 15 = PNG prediction (on encoding, PNG optimum)
        /// </summary>
        public int Predictor { get; init; } = 1;

        public int Colors { get; init; } = 1;

        // Valid values are 1, 2, 4, 8, and (PDF 1.5) 16
        public int BitsPerComponent { get; init; } = 8;

        public int Columns { get; init; } = 1;
    }

    internal class LZWFilterParams : FlateFilterParams
    {
        public int EarlyChange { get; init; } = 1;
    }
}
