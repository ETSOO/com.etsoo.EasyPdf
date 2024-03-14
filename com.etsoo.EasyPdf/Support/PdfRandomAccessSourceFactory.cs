namespace com.etsoo.EasyPdf.Support
{
    internal static class PdfRandomAccessSourceFactory
    {
        public static IPdfRandomAccessSource CreateSource(byte[] data)
        {
            return new PdfArrayRandomAccessSource(data);
        }

        public static IPdfRandomAccessSource CreateSource(string file, bool forceRead = false)
        {
            return CreateSource(new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read), forceRead);
        }

        public static IPdfRandomAccessSource CreateSource(Stream stream, bool forceRead = false)
        {
            if (forceRead || !stream.CanSeek)
            {
                return CreateSource(stream.ToBytes().ToArray());
            }
            else
            {
                return new PdfRAFRandomAccessSource(stream);
            }
        }

        public static IPdfRandomAccessSource CreateRanged(IPdfRandomAccessSource source, IList<long> ranges)
        {
            var sources = new IPdfRandomAccessSource[ranges.Count/2];
            for (int i = 0; i < ranges.Count; i+=2)
            {
                sources[i/2] = new PdfWindowRandomAccessSource(source, ranges[i], ranges[i+1]);
            }
            return new PdfGroupedRandomAccessSource(sources);
        }
    }
}
