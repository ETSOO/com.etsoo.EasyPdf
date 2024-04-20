using System.IO.Compression;

namespace com.etsoo.EasyPdf.Filters
{
    /// <summary>
    /// Flate is basically Zip compression; so, any content streams which are not compressed will be compressed using Flate
    /// </summary>
    internal class FlateFilter : IFilter
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
        /// Name
        /// 名称
        /// </summary>
        public string Name => "FlateDecode";

        public FlateFilter(FlateFilterParams? parameters = null)
        {

        }

        /// <summary>
        /// Encode data
        /// 编码数据
        /// </summary>
        /// <param name="data">Data</param>
        /// <returns>Result</returns>
        public ReadOnlySpan<byte> Encode(ReadOnlySpan<byte> data)
        {
            using var stream = PdfConstants.StreamManager.GetStream();
            SetupCompressStream(stream);

            using var zip = new DeflateStream(stream, CompressionMode.Compress);
            zip.Write(data);
            zip.Flush();

            stream.Position = 0;

            return stream.ToBytes().Span;
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
        /// <returns>Result</returns>
        public async Task<ReadOnlyMemory<byte>> EncodeAsync(ReadOnlyMemory<byte> data)
        {
            await using var stream = PdfConstants.StreamManager.GetStream();
            SetupCompressStream(stream);

            await using var zip = new DeflateStream(stream, CompressionMode.Compress);
            await zip.WriteAsync(data);
            await zip.FlushAsync();

            stream.Position = 0;

            return await stream.ToBytesAsync();
        }

        /// <summary>
        /// Decode data
        /// 解码数据
        /// </summary>
        /// <param name="data">Data</param>
        /// <returns>Result</returns>
        public ReadOnlySpan<byte> Decode(ReadOnlySpan<byte> data)
        {
            // Skin first two bytes (CMF & FLG)
            data = data[2..];

            using var inputStream = PdfConstants.StreamManager.GetStream(data);
            using var zip = new DeflateStream(inputStream, CompressionMode.Decompress);
            zip.Flush();

            return zip.ToBytes().Span;
        }

        /// <summary>
        /// Async decode data
        /// 异步解码数据
        /// </summary>
        /// <param name="data">Data</param>
        /// <returns>Result</returns>
        public async Task<ReadOnlyMemory<byte>> DecodeAsync(ReadOnlyMemory<byte> data)
        {
            // Skin first two bytes (CMF & FLG)
            data = data[2..];

            await using var inputStream = PdfConstants.StreamManager.GetStream(data.Span);
            await using var zip = new DeflateStream(inputStream, CompressionMode.Decompress);
            await zip.FlushAsync();

            return await zip.ToBytesAsync();
        }
    }
}
