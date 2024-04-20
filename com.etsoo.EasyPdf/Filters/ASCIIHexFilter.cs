namespace com.etsoo.EasyPdf.Filters
{
    internal class ASCIIHexFilter : IFilter
    {
        private static byte EncodeByte(int b)
        {
            return (byte)(b + (b < 10 ? '0' : 'A' - 10));
        }

        private static byte DecodeByte(int b)
        {
            return (byte)(b > '9' ? b - '7' : b - '0');
        }

        /// <summary>
        /// Name
        /// 名称
        /// </summary>
        public string Name => "ASCIIHexDecode";

        /// <summary>
        /// Encode data
        /// 编码数据
        /// </summary>
        /// <param name="data">Data</param>
        /// <returns>Result</returns>
        public ReadOnlySpan<byte> Encode(ReadOnlySpan<byte> data)
        {
            var count = data.Length;
            Span<byte> bytes = new byte[2 * count];
            for (int i = 0, j = 0; i < count; i++)
            {
                var b = data[i];
                bytes[j++] = EncodeByte(b >> 4);
                bytes[j++] = EncodeByte(b & 0xF);
            }
            return bytes;
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
            var count = data.Length;

            // Ignore EOD (end of data) character
            // For performance consideration, only detects last byte
            if (data[count - 1] == PdfConstants.GreaterThanSignByte)
            {
                count--;
                data = data[..count];
            }

            if (count % 2 == 1)
            {
                // If the final digit of a hexadecimal string is missing—that is,
                // if there is an odd number of digits—the final digit shall be assumed to be 0
                count++;
                Span<byte> temp = new byte[count];
                data.CopyTo(temp);
                data = temp;
            }

            count >>= 1;

            Span<byte> bytes = new byte[count];
            for (int i = 0, j = 0; i < count; i++)
            {
                byte hb = data[j++];
                byte lb = data[j++];
                bytes[i] = (byte)(DecodeByte(hb) * 16 + DecodeByte(lb));
            }

            return bytes;
        }
    }
}
