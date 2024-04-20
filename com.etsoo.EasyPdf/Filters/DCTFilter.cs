
namespace com.etsoo.EasyPdf.Filters
{
    /// <summary>
    /// DCT filter
    /// </summary>
    internal class DCTFilter : IFilter
    {
        public string Name => "DCTDecode";

        public ReadOnlySpan<byte> Decode(ReadOnlySpan<byte> data)
        {
            return data;
        }

        public ReadOnlySpan<byte> Encode(ReadOnlySpan<byte> data)
        {
            return data;
        }

        public async Task<ReadOnlyMemory<byte>> EncodeAsync(ReadOnlyMemory<byte> data)
        {
            await Task.CompletedTask;
            return data;
        }
    }
}
