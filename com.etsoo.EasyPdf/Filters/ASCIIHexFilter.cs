namespace com.etsoo.EasyPdf.Filters
{
    internal class ASCIIHexFilter : IFilter<FilterParams>
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
        /// Encode data
        /// 编码数据
        /// </summary>
        /// <param name="data">Data</param>
        /// <param name="parameters">Parameters</param>
        /// <returns>Result</returns>
        public ReadOnlySpan<byte> Encode(ReadOnlySpan<byte> data, FilterParams? parameters = null)
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
        /// Decode data
        /// 解码数据
        /// </summary>
        /// <param name="data">Data</param>
        /// <param name="parameters">Parameters</param>
        /// <returns>Result</returns>
        public ReadOnlySpan<byte> Decode(ReadOnlySpan<byte> data, FilterParams? parameters = null)
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
