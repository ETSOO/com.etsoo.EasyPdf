using System.IO;

namespace com.etsoo.EasyPdf.IO
{
    internal static class PdfRandomAccessSourceFactory
    {
        public static IPdfRandomAccessSource CreateSource(byte[] data)
        {
            return new PdfArrayRandomAccessSource(data);
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
    }
}
