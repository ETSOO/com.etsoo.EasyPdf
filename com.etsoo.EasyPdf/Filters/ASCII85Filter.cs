using CommunityToolkit.HighPerformance;
using CommunityToolkit.HighPerformance.Buffers;
using System.Buffers;

namespace com.etsoo.EasyPdf.Filters
{
    /// <summary>
    /// Remove ASCII-85 – An older style encoding which allowed early PDF files to be transferred and stored on systems which used 7 bit ASCII.
    /// This type of encoding is no longer necessary and actually expands the size of the content streams
    /// </summary>
    internal class ASCII85Filter : IFilter<FilterParams>
    {
        /// <summary>
        /// Encode data
        /// 编码数据
        /// </summary>
        /// <param name="data">Data</param>
        /// <param name="parameters">Parameters</param>
        /// <returns>Result</returns>
        public ReadOnlySpan<byte> Encode(ReadOnlySpan<byte> data, FilterParams? parameters = null)
        {
            var length = data.Length;
            var words = length / 4;
            var rest = length - (words * 4);
            Span<byte> result = new byte[words * 5 + (rest == 0 ? 0 : rest + 1) + 2];

            int idxIn = 0, idxOut = 0;
            int wCount = 0;
            while (wCount < words)
            {
                var val = ((uint)data[idxIn++] << 24) + ((uint)data[idxIn++] << 16) + ((uint)data[idxIn++] << 8) + data[idxIn++];
                if (val == 0)
                {
                    result[idxOut++] = (byte)'z';
                }
                else
                {
                    var c5 = (byte)(val % 85 + '!');
                    val /= 85;
                    var c4 = (byte)(val % 85 + '!');
                    val /= 85;
                    var c3 = (byte)(val % 85 + '!');
                    val /= 85;
                    var c2 = (byte)(val % 85 + '!');
                    val /= 85;
                    var c1 = (byte)(val + '!');

                    result[idxOut++] = c1;
                    result[idxOut++] = c2;
                    result[idxOut++] = c3;
                    result[idxOut++] = c4;
                    result[idxOut++] = c5;
                }
                wCount++;
            }
            if (rest == 1)
            {
                var val = (uint)data[idxIn] << 24;
                val /= 85 * 85 * 85;
                var c2 = (byte)(val % 85 + '!');
                val /= 85;
                var c1 = (byte)(val + '!');

                result[idxOut++] = c1;
                result[idxOut++] = c2;
            }
            else if (rest == 2)
            {
                var val = ((uint)data[idxIn++] << 24) + ((uint)data[idxIn] << 16);
                val /= 85 * 85;
                var c3 = (byte)(val % 85 + '!');
                val /= 85;
                var c2 = (byte)(val % 85 + '!');
                val /= 85;
                var c1 = (byte)(val + '!');

                result[idxOut++] = c1;
                result[idxOut++] = c2;
                result[idxOut++] = c3;
            }
            else if (rest == 3)
            {
                var val = ((uint)data[idxIn++] << 24) + ((uint)data[idxIn++] << 16) + ((uint)data[idxIn] << 8);
                val /= 85;
                var c4 = (byte)(val % 85 + '!');
                val /= 85;
                var c3 = (byte)(val % 85 + '!');
                val /= 85;
                var c2 = (byte)(val % 85 + '!');
                val /= 85;
                var c1 = (byte)(val + '!');

                result[idxOut++] = c1;
                result[idxOut++] = c2;
                result[idxOut++] = c3;
                result[idxOut++] = c4;
            }
            result[idxOut++] = (byte)'~';
            result[idxOut++] = (byte)'>';

            if (idxOut < result.Length)
                return result[..idxOut];

            return result;
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
            // Remove all white-space characters
            var pos = data.IndexOfAny(PdfConstants.WhiteSpaceCharacters);
            if (pos != -1)
            {
                var sw = new ArrayPoolBufferWriter<byte>();

                while (pos != -1)
                {
                    sw.Write(data[..pos]);
                    data = data[(pos + 1)..];
                    pos = data.IndexOfAny(PdfConstants.WhiteSpaceCharacters);
                }

                sw.Write(data);

                data = sw.WrittenSpan;
            }

            var length = data.Length;
            if (data[0] == '<' && data[1] == '~')
            {
                length -= 2;
                data = data[2..];
            }
            if (data[length - 2] == '~' && data[length - 1] == '>')
            {
                length -= 2;
                data = data[..length];
            }

            var w = new ArrayPoolBufferWriter<byte>();

            var idx = 0;
            while (idx + 4 < length)
            {
                if (data[idx] == (byte)'z')
                {
                    // Zero
                    idx++;
                    w.GetSpan(4).Fill((byte)'0');
                }
                else
                {
                    var value =
                      (uint)(data[idx++] - '!') * (85 * 85 * 85 * 85) +
                      (uint)(data[idx++] - '!') * (85 * 85 * 85) +
                      (uint)(data[idx++] - '!') * (85 * 85) +
                      (uint)(data[idx++] - '!') * 85 +
                      (uint)(data[idx++] - '!');

                    w.Write(new byte[] { (byte)(value >> 24), (byte)(value >> 16), (byte)(value >> 8), (byte)value });
                }
            }

            var remainder = length - idx;

            if (remainder == 2) // one byte
            {
                var value =
                  (uint)(data[idx++] - '!') * (85 * 85 * 85 * 85) +
                  (uint)(data[idx] - '!') * (85 * 85 * 85);

                // Always increase if not zero (tried out).
                if (value != 0)
                    value += 0x01000000;

                w.Write((byte)(value >> 24));
            }
            else if (remainder == 3) // two bytes
            {
                var idxIn = idx;
                var value =
                  (uint)(data[idx++] - '!') * (85 * 85 * 85 * 85) +
                  (uint)(data[idx++] - '!') * (85 * 85 * 85) +
                  (uint)(data[idx] - '!') * (85 * 85);

                if (value != 0)
                {
                    value &= 0xFFFF0000;
                    var val = value / (85 * 85);
                    var c3 = (byte)(val % 85 + '!');
                    val /= 85;
                    var c2 = (byte)(val % 85 + '!');
                    val /= 85;
                    var c1 = (byte)(val + '!');
                    if (c1 != data[idxIn] || c2 != data[idxIn + 1] || c3 != data[idxIn + 2])
                    {
                        value += 0x00010000;
                    }
                }

                w.Write(new byte[] { (byte)(value >> 24), (byte)(value >> 16) });
            }
            else if (remainder == 4) // three bytes
            {
                var idxIn = idx;
                var value =
                  (uint)(data[idx++] - '!') * (85 * 85 * 85 * 85) +
                  (uint)(data[idx++] - '!') * (85 * 85 * 85) +
                  (uint)(data[idx++] - '!') * (85 * 85) +
                  (uint)(data[idx] - '!') * 85;

                if (value != 0)
                {
                    value &= 0xFFFFFF00;
                    var val = value / 85;
                    var c4 = (byte)(val % 85 + '!');
                    val /= 85;
                    var c3 = (byte)(val % 85 + '!');
                    val /= 85;
                    var c2 = (byte)(val % 85 + '!');
                    val /= 85;
                    var c1 = (byte)(val + '!');
                    if (c1 != data[idxIn] || c2 != data[idxIn + 1] || c3 != data[idxIn + 2] || c4 != data[idxIn + 3])
                    {
                        value += 0x00000100;
                    }
                }
                w.Write(new byte[] { (byte)(value >> 24), (byte)(value >> 16), (byte)(value >> 8) });
            }

            return w.WrittenSpan;
        }
    }
}
