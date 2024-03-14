using CommunityToolkit.HighPerformance.Buffers;
using System.Buffers;
using System.Collections;

namespace com.etsoo.EasyPdf.Filters
{
    internal class LZWDictionary
    {
        private static ReadOnlySpan<byte> BitsToBytes(BitArray bits)
        {
            var count = (bits.Length - 1) / 8 + 1;
            bits.Length = 8 * count;

            var w = new ArrayPoolBufferWriter<byte>(count);

            var i = 0;
            while (i < bits.Length)
            {
                var b = new BitArray(8);
                for (var j = 0; j < 8; j++)
                {
                    // Reverse
                    // because of high-order bit first
                    b[7-j] = bits[i++];
                }
                var bs = new byte[1];
                b.CopyTo(bs, 0);
                w.Write(bs);
            }

            return w.WrittenSpan;
        }

        private const ushort ClearTableMarker = 256;
        private const ushort EODMarker = 257;

        private byte[][] Items = default!;

        // Support to 16 bits (default is 12)
        private readonly ushort MaxCodes;

        // Dictionary index
        private int dicIndex;

        public LZWDictionary(bool LZC = false)
        {
            MaxCodes = LZC ? (ushort)65535 : (ushort)4095;

            // Setup
            Reset();
        }

        private int GetCodeBytes(int delayedStep = 0)
        {
            // dicIndex is the next available index (last index + 1)
            return (dicIndex + delayedStep) switch
            {
                < 512 => 9,
                < 1024 => 10,
                < 2048 => 11,
                < 4096 => 12,
                < 8192 => 13,
                < 16384 => 14,
                < 32768 => 15,
                _ => 16
            };
        }

        private void AddCode(BitArray bits, int code)
        {
            var cb = GetCodeBytes();

            var pos = bits.Length;
            bits.Length = pos + cb;

            var codeBits = new BitArray(new[] { code })
            {
                Length = cb
            };

            // Codes shall be packed into a continuous bit stream, high-order bit first
            for (var i = cb - 1; i >= 0; i--)
            {
                bits[pos++] = codeBits[i];
            }
        }

        private void Reset()
        {
            Items = new byte[MaxCodes][];

            // Simple and basic dictionary to start
            // Each code shall represent a single character of input data (0–255)
            for (var i = 0; i < 256; i++)
            {
                Items[i] = [(byte)i];
            }

            // 256 - Clear table marker
            Items[256] = [0];

            // 257 - EOD marker
            Items[257] = [0];

            dicIndex = EODMarker + 1;
        }

        private void AddEntry(byte[] entry)
        {
            Items[dicIndex++] = entry;
        }

        public ReadOnlySpan<byte> Encode(ReadOnlySpan<byte> data)
        {
            // Results
            var bits = new BitArray(0);

            // Add clear table mark
            AddCode(bits, ClearTableMarker);

            // Previous collection
            var p = new List<byte>();

            // Last match index
            var lastIndex = 0;

            var dataLen = data.Length;
            for (var i = 0; i < dataLen; i++)
            {
                // c for current data
                var c = data[i];

                // p + c
                p.Add(c);

                // Exists in the dictionary?
                // First time alwasy be true
                var index = Array.FindIndex(Items, (item) => item != null && item.SequenceEqual(p));
                if (index != -1)
                {
                    lastIndex = index;

                    // Continue to look for longer existance
                    continue;
                }

                // Add code
                AddCode(bits, lastIndex);

                // Add new entry
                AddEntry([.. p]);

                // When not the last byte, add clear table marker
                if (dicIndex == MaxCodes && i + 1 < dataLen)
                {
                    AddCode(bits, ClearTableMarker);
                    Reset();
                }

                // Reset previous collection to current data
                p.Clear();

                // Back one byte
                i--;
            }

            // Last byte match
            // P is blank when data is empty
            if (data.Length > 0)
            {
                int endIndex;
                if (p.Count == 0)
                {
                    endIndex = dicIndex -1;
                }
                else
                {
                    endIndex = lastIndex;
                }
                AddCode(bits, endIndex);
            }

            // Complete
            AddCode(bits, EODMarker);

            // Console.WriteLine($"Encode {string.Join(' ', Items.Select(item => Encoding.ASCII.GetString(item)))}");

            // Return
            return BitsToBytes(bits);
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
            var w = new ArrayPoolBufferWriter<byte>();

            var bits = new BitArray(data.Length * 8);

            var i = 0;
            foreach (var c in data)
            {
                var b = new BitArray(new[] { c });
                for (var j = 8 - 1; j >= 0; j--)
                {
                    bits[i++] = b[j];
                }
            }

            var h = 0;
            var lastBytes = Array.Empty<byte>();
            var bcount = bits.Length;
            while (h < bcount)
            {
                var cb = GetCodeBytes(1);

                var codeBytes = new BitArray(cb);
                for (var c = cb - 1; c >= 0; c--)
                {
                    codeBytes[c] = h < bcount && bits[h++];
                }

                var codes = new int[1];
                codeBytes.CopyTo(codes, 0);
                var code = codes.First();

                if (code == ClearTableMarker)
                {
                    Reset();
                    continue;
                }

                if (code == EODMarker)
                {
                    break;
                }

                if (code < dicIndex)
                {
                    var bs = Items[code];
                    w.Write(bs);

                    if (lastBytes.Length > 0)
                    {
                        var p = new List<byte>(lastBytes)
                        {
                            bs[0]
                        };

                        // Add new entry
                        AddEntry([.. p]);
                    }
                    lastBytes = bs;
                }
                else
                {
                    var p = new List<byte>(lastBytes);
                    p.Add(p.First());

                    lastBytes = [.. p];

                    w.Write(lastBytes);

                    // Add new entry
                    AddEntry(lastBytes);
                }
            }

            return w.WrittenSpan;
        }
    }
}
