namespace com.etsoo.EasyPdf.Filters
{
    internal interface IFilter
    {
        /// <summary>
        /// Name
        /// 名称
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Encode data
        /// 编码数据
        /// </summary>
        /// <param name="data">Data</param>
        /// <returns>Result</returns>
        ReadOnlySpan<byte> Encode(ReadOnlySpan<byte> data);

        /// <summary>
        /// Async encode data
        /// 异步编码数据
        /// </summary>
        /// <param name="data">Data</param>
        /// <returns>Result</returns>
        Task<ReadOnlyMemory<byte>> EncodeAsync(ReadOnlyMemory<byte> data);

        /// <summary>
        /// Decode data
        /// 解码数据
        /// </summary>
        /// <param name="data">Data</param>
        /// <returns>Result</returns>
        ReadOnlySpan<byte> Decode(ReadOnlySpan<byte> data);
    }
}
