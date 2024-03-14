using System.Text;

namespace com.etsoo.EasyPdf.Types
{
    internal class PdfEOF
    {
        /// <summary>
        /// End of File prefix
        /// </summary>
        private static readonly byte[] Prefix = Encoding.ASCII.GetBytes("%%EOF");

        public static PdfEOF? Parse(ReadOnlySpan<byte> bytes)
        {
            if (bytes.SequenceEqual(Prefix))
            {
                return new PdfEOF();
            }
            return null;
        }

        public async Task WriteToAsync(Stream stream)
        {
            await stream.WriteAsync(Prefix);
        }
    }
}
