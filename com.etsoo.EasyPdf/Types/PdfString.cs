using CommunityToolkit.HighPerformance;
using CommunityToolkit.HighPerformance.Buffers;
using System.Buffers;
using System.Globalization;
using System.Text;

namespace com.etsoo.EasyPdf.Types
{
    /// <summary>
    /// PDF string
    /// PDF 字符串
    /// </summary>
    internal record PdfString(string Value) : IPdfType<string>
    {
        private static byte? GetOctalByte(byte[] octalBytes)
        {
            // Like '053' = '+'
            var str = Encoding.ASCII.GetString(octalBytes);

            try
            {
                return Convert.ToByte(str, 8);
            }
            catch
            {
                return null;
            }
        }

        public bool KeyEquals(string item)
        {
            return Value.Equals(item);
        }

        private static void WriteOctalBytes(List<byte> escapingDigits, ArrayPoolBufferWriter<byte> w)
        {
            var ob = GetOctalByte(escapingDigits.ToArray());
            if (ob is not null)
            {
                w.Write(ob.Value);
            }
            escapingDigits.Clear();
        }

        public static IPdfType Parse(ReadOnlySpan<byte> bytes)
        {
            var w = new ArrayPoolBufferWriter<byte>();

            var leftParentheses = 0;
            var escaping = false;
            var escaped = false;
            var escapingDigits = new List<byte>();

            for (var i = 0; i < bytes.Length; i++)
            {
                var one = bytes[i];
                if (one == PdfConstants.LeftParenthesisByte)
                {
                    if (escaping)
                    {
                        escaping = false;
                        if (escapingDigits.Count > 0)
                        {
                            WriteOctalBytes(escapingDigits, w);
                        }
                        else
                        {
                            w.Write(one);
                            continue;
                        }
                    }
                    w.Write(one);
                    leftParentheses++;
                    continue;
                }

                if (one == PdfConstants.RightParenthesisByte)
                {
                    if (escaping)
                    {
                        escaping = false;
                        if (escapingDigits.Count > 0)
                        {
                            WriteOctalBytes(escapingDigits, w);
                        }
                        else
                        {
                            w.Write(one);
                            continue;
                        }
                    }

                    if (leftParentheses > 0)
                    {
                        // Balanced parenthesis
                        w.Write(one);
                        leftParentheses--;
                    }
                    else
                    {
                        // Complete
                        break;
                    }

                    continue;
                }

                if (one == PdfConstants.ReverseSolidusByte)
                {
                    if (escaping)
                    {
                        escaping = false;

                        if (escapingDigits.Count > 0)
                        {
                            WriteOctalBytes(escapingDigits, w);
                        }
                        else
                        {
                            w.Write(one);
                            continue;
                        }
                    }

                    escaping = true;
                    escaped = true;

                    continue;
                }

                if (escaping)
                {
                    // 0 - 9
                    if (one is >= 48 and <= 57)
                    {
                        escapingDigits.Add(one);
                        if (escapingDigits.Count == 3)
                        {
                            WriteOctalBytes(escapingDigits, w);
                            escaping = false;
                        }
                        continue;
                    }

                    if (one is PdfConstants.CarriageReturnByte or PdfConstants.LineFeedByte)
                    {
                        // Ignore line-breaks
                        var next = bytes[i + 1];

                        if (next is not (PdfConstants.CarriageReturnByte or PdfConstants.LineFeedByte))
                            escaping = false;

                        continue;
                    }

                    byte? eb = one switch
                    {
                        (byte)'n' => PdfConstants.LineFeedByte,
                        (byte)'r' => PdfConstants.CarriageReturnByte,
                        (byte)'t' => PdfConstants.HorizontalTabByte,
                        (byte)'b' => PdfConstants.SpaceByte,
                        (byte)'f' => PdfConstants.FormFeedByte,
                        _ => null
                    };

                    escaping = false;

                    if (eb is not null)
                    {
                        w.Write(eb.Value);
                    }
                    else if (escapingDigits.Count > 0)
                    {
                        WriteOctalBytes(escapingDigits, w);
                    }
                    continue;
                }

                w.Write(one);
            }

            // Literal strings
            var value = Encoding.UTF8.GetString(w.WrittenSpan);

            // UTF8 string
            if (!escaped && value.StartsWith("D:") && DateTime.TryParseExact(value[2..].Replace('\'', ':'), PdfDateTime.Formats, null, DateTimeStyles.AssumeLocal | DateTimeStyles.NoCurrentDateDefault, out var dt))
            {
                // Date Time
                return new PdfDateTime(dt);
            }

            return new PdfString(value);
        }

        public async Task WriteToAsync(Stream stream)
        {
            // Literal strings;
            stream.WriteByte(PdfConstants.LeftParenthesisByte);

            // Replace
            var w = new ArrayPoolBufferWriter<byte>();
            foreach (var b in Encoding.UTF8.GetBytes(Value))
            {
                if (b >= 127)
                {
                    var output = "\\" + Convert.ToString(b, 8).PadLeft(3, '0');
                    w.Write(Encoding.ASCII.GetBytes(output));
                    continue;
                }

                if (b is PdfConstants.ReverseSolidusByte or PdfConstants.LeftParenthesisByte or PdfConstants.RightParenthesisByte)
                {
                    // Lazy way to avoid balance parenthesis
                    w.Write(PdfConstants.ReverseSolidusByte);
                }

                w.Write(b);
            }

            await stream.WriteAsync(w.WrittenMemory);

            stream.WriteByte(PdfConstants.RightParenthesisByte);
        }
    }
}
