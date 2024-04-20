namespace com.etsoo.EasyPdf.Filters
{
    /// <summary>
    ///  The use of LZW fell out of favor because of licensing issues
    ///  The algorithm has been patented, but all patents have expired by now
    ///  Data encoded using the LZW compression method shall consist of a sequence of codes that are 9 to 12 bits long
    ///  LZC - the implementation of the algorithm in the compress utility limits the maximum dictionary size to 16 bits
    ///  LZT - on overflow, removes from the dictionary a phrase that has not been used for the longest time
    ///  https://segmentfault.com/a/1190000011425787
    /// </summary>
    internal class LZWFilter : IFilter
    {
        /// <summary>
        /// Name
        /// 名称
        /// </summary>
        public string Name => "LZWDecode";

        public LZWFilter(LZWFilterParams? parameters = null)
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
            // Dictionary
            var dic = new LZWDictionary();

            // Encode
            return dic.Encode(data);
        }

        /// <summary>
        /// Async encode data
        /// 异步编码数据
        /// </summary>
        /// <param name="data">Data</param>
        /// <returns>Result</returns>
        public async Task<ReadOnlyMemory<byte>> EncodeAsync(ReadOnlyMemory<byte> data)
        {
            await Task.CompletedTask;

            var memory = new Memory<byte>();
            Encode(data.Span).CopyTo(memory.Span);
            return memory;
        }

        /// <summary>
        /// Decode data
        /// 解码数据
        /// </summary>
        /// <param name="data">Data</param>
        /// <returns>Result</returns>
        public ReadOnlySpan<byte> Decode(ReadOnlySpan<byte> data)
        {
            // Dictionary
            var dic = new LZWDictionary();

            // Encode
            return dic.Decode(data);
        }
    }
}
