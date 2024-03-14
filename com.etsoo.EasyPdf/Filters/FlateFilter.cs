using System.IO.Compression;

namespace com.etsoo.EasyPdf.Filters
{
    /// <summary>
    /// Flate is basically Zip compression; so, any content streams which are not compressed will be compressed using Flate
    /// </summary>
    internal class FlateFilter : IFilter<FlateFilterParams>
    {
        // CMF - Low 4-bit compression method (default 8 means deflate);
        //       High 4-bit information field depending on the compression method (default 7, 32K window size), byte = 120 (01111000)
        // FLG - bits 0 to 4  FCHECK  (check bits for CMF and FLG), (120 * 256 + 120 (10000000) + FCHECK) % 31 = 0
        //       bit  5       FDICT   (preset dictionary)
        //       bits 6 to 7  FLEVEL  (compression level, 2 is default), byte = 156 (10011100)
        //          0 - compressor used fastest algorithm
        //          1 - compressor used fast algorithm
        //          2 - compressor used default algorithm
        //          3 - compressor used maximum compression, slowest algorithm
        // http://www.faqs.org/rfcs/rfc1950.html

        /// <summary>
        /// Encode data
        /// 编码数据
        /// </summary>
        /// <param name="data">Data</param>
        /// <param name="parameters">Parameters</param>
        /// <returns>Result</returns>
        public ReadOnlySpan<byte> Encode(ReadOnlySpan<byte> data, FlateFilterParams? parameters = null)
        {
            using var stream = PdfConstants.StreamManager.GetStream();
            SetupCompressStream(stream);

            using var zip = new DeflateStream(stream, CompressionMode.Compress, true);
            zip.Write(data);
            return stream.ToArray();
        }

        private void SetupCompressStream(Stream stream)
        {
            // Default CMF
            stream.WriteByte(120);

            // Default FLG
            stream.WriteByte(156);
        }

        /// <summary>
        /// Async encode data
        /// 异步编码数据
        /// </summary>
        /// <param name="data">Data</param>
        /// <param name="parameters">Parameters</param>
        /// <returns>Result</returns>
        public async Task<MemoryStream> EncodeAsync(ReadOnlyMemory<byte> data, FlateFilterParams? parameters = null)
        {
            var stream = PdfConstants.StreamManager.GetStream();
            SetupCompressStream(stream);

            await using var zip = new DeflateStream(stream, CompressionMode.Compress, true);
            await zip.WriteAsync(data);
            return stream;
        }

        /// <summary>
        /// Decode data
        /// 解码数据
        /// </summary>
        /// <param name="data">Data</param>
        /// <param name="parameters">Parameters</param>
        /// <returns>Result</returns>
        public ReadOnlySpan<byte> Decode(ReadOnlySpan<byte> data, FlateFilterParams? parameters = null)
        {
            // Skin first two bytes (CMF & FLG)
            data = data[2..];

            using var inputStream = PdfConstants.StreamManager.GetStream(data);
            using var stream = PdfConstants.StreamManager.GetStream();
            using var zip = new DeflateStream(inputStream, CompressionMode.Decompress);

            zip.CopyTo(stream);
            return stream.ToArray();
        }

        /// <summary>
        /// Async decode data
        /// 异步解码数据
        /// </summary>
        /// <param name="data">Data</param>
        /// <param name="parameters">Parameters</param>
        /// <returns>Result</returns>
        public async Task<MemoryStream> DecodeAsync(ReadOnlyMemory<byte> data, FlateFilterParams? parameters = null)
        {
            var stream = PdfConstants.StreamManager.GetStream();
            using var inputStream = PdfConstants.StreamManager.GetStream(data.Span);
            using var zip = new DeflateStream(inputStream, CompressionMode.Decompress);
            await zip.CopyToAsync(stream);
            return stream;
        }
    }
}
