namespace com.etsoo.EasyPdf.Filters
{
    internal interface IFilter<P> where P : FilterParams
    {
        /// <summary>
        /// Encode data
        /// 编码数据
        /// </summary>
        /// <param name="data">Data</param>
        /// <param name="parameters">Parameters</param>
        /// <returns>Result</returns>
        ReadOnlySpan<byte> Encode(ReadOnlySpan<byte> data, P? parameters = null);

        /// <summary>
        /// Decode data
        /// 解码数据
        /// </summary>
        /// <param name="data">Data</param>
        /// <param name="parameters">Parameters</param>
        /// <returns>Result</returns>
        ReadOnlySpan<byte> Decode(ReadOnlySpan<byte> data, P? parameters = null);
    }
}
