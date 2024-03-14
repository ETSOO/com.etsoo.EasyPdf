using com.etsoo.EasyPdf.Filters;
using com.etsoo.EasyPdf.Support;
using com.etsoo.EasyPdf.Types;
using CommunityToolkit.HighPerformance.Buffers;
using System.Drawing;
using System.Text;

namespace com.etsoo.EasyPdf
{
    /// <summary>
    /// PDF extensions
    /// PDF 扩展
    /// </summary>
    internal static class PdfExtensions
    {
        /// <summary>
        /// Is binary check bytes
        /// 是否为二进制检查字节
        /// </summary>
        /// <param name="bytes">Bytes</param>
        /// <returns>Result</returns>
        public static bool IsBinaryCheck(this ReadOnlySpan<byte> bytes)
        {
            if (bytes.Length < 4) return false;

            foreach (var b in bytes)
            {
                if (b <= 127) return false;
            }

            return true;
        }

        /// <summary>
        /// Is binary check bytes
        /// 是否为二进制检查字节
        /// </summary>
        /// <param name="bytes">Bytes</param>
        /// <returns>Result</returns>
        public static bool IsAllAscii(this ReadOnlySpan<char> chars)
        {
            foreach (var c in chars)
            {
                if (!char.IsAscii(c)) return false;
            }

            return true;
        }

        /// <summary>
        /// Get string from Hex coded bytes
        /// 从十六进制编码字节中获取字符串
        /// </summary>
        /// <param name="bytes">Hex coded bytes</param>
        /// <param name="encoding">Text encoding detected</param>
        /// <returns>Result</returns>
        public static string FromHexCoded(this ReadOnlySpan<byte> bytes, out Encoding encoding)
        {
            byte bom;
            (encoding, bom) = PdfEncoding.DetectHex(bytes);

            var filter = new ASCIIHexFilter();
            return encoding.GetString(filter.Decode(bytes[bom..]));
        }

        public static IPdfType[] Parse(this ReadOnlySpan<byte> data)
        {
            var items = new List<IPdfType>();

            var i = 0;
            var len = data.Length;
            while (i < len)
            {
                var b = data[i];

                if (b == 110)
                {
                    // null
                    // n = 110
                    items.Add(new PdfNull());
                    i += 4;
                    continue;
                }

                if (b == 116)
                {
                    // true
                    // t = 116
                    items.Add(new PdfBoolean(true));
                    i += 4;
                    continue;
                }

                if (b == 102)
                {
                    // false
                    // f = 102
                    items.Add(new PdfBoolean(false));
                    i += 5;
                    continue;
                }

                // +, -, 0 - 9
                if (b is 43 or 45 or (>= 48 and <= 57))
                {
                    var nextData = data[i..];
                    var pos = nextData.IndexOfAny(PdfConstants.DelimiterStartCharactersWithAllSpaces);
                    if (pos == -1)
                    {
                        var r = PdfInt.Parse(nextData);
                        items.Add(r);
                        i = len;
                    }
                    else
                    {
                        // Identify object case
                        // 1 0 R
                        var nextLen = nextData.Length;
                        if (b is >= 49 and <= 57 && nextData[pos] == PdfConstants.SpaceByte && (pos + 1) < nextLen && nextData[pos + 1] is >= 48 and <= 57)
                        {
                            var testData = pos + 8 >= nextLen ? nextData[(pos + 1)..] : nextData[(pos + 1)..(pos + 8)];
                            var testPos = testData.IndexOf(PdfConstants.SpaceByte);
                            if (testPos > 0 && testData.IndexOf(PdfConstants.ReferenceByte) == testPos + 1)
                            {
                                var extend = pos + 1 + testPos + 2;

                                var reference = PdfObject.Parse(nextData[..extend], true);
                                items.Add(reference);

                                i += extend;
                                continue;
                            }
                        }
                        var r = PdfInt.Parse(nextData[..pos]);
                        items.Add(r);
                        i += pos;
                    }
                    continue;
                }

                if (b == PdfConstants.Solidus)
                {
                    // Name object /(...)
                    // Any delimiters and space
                    var nextData = data[(i + 1)..];
                    var pos = nextData.IndexOfAny(PdfConstants.DelimiterStartCharactersWithAllSpaces);
                    if (pos == -1)
                    {
                        var r = PdfName.Parse(nextData);
                        items.Add(r);
                        i = len;
                    }
                    else
                    {
                        var r = PdfName.Parse(nextData[..pos]);
                        items.Add(r);
                        i += pos + 1;
                    }
                    continue;
                }

                if (b == PdfConstants.LessThanSignByte && data[i + 1] != PdfConstants.LessThanSignByte)
                {
                    // Binary string <...>
                    var nextData = data[(i + 1)..];
                    var pos = nextData.IndexOf(PdfConstants.GreaterThanSignByte);
                    if (pos == -1)
                    {
                        throw new InvalidDataException("No binary string ending boundary '>'");
                    }

                    var r = PdfBinaryString.Parse(nextData[..pos]);
                    items.Add(r);
                    i += pos + 1;

                    continue;
                }

                if (b == PdfConstants.LeftParenthesisByte)
                {
                    // String (...)
                    var pos = -1;
                    var leftCount = 0;
                    var start = i + 1;
                    for (var n = start; n < len; n++)
                    {
                        if (data[n] == PdfConstants.LeftParenthesisByte && data[n - 1] != PdfConstants.ReverseSolidusByte)
                        {
                            leftCount++;
                            continue;
                        }

                        if (data[n] == PdfConstants.RightParenthesisByte && data[n - 1] != PdfConstants.ReverseSolidusByte)
                        {
                            if (leftCount == 0)
                            {
                                pos = n;
                                break;
                            }

                            leftCount--;
                            continue;
                        }
                    }

                    if (pos == -1)
                    {
                        throw new InvalidDataException("No string ending boundary ')'");
                    }

                    var r = PdfString.Parse(data[start..pos]);
                    items.Add(r);
                    i = pos + 1;

                    continue;
                }

                if (b == PdfConstants.LeftSquareBracketByte)
                {
                    // Array [...]
                    var pos = -1;
                    var leftCount = 0;
                    var start = i + 1;
                    for (var n = start; n < len; n++)
                    {
                        if (data[n] == PdfConstants.LeftSquareBracketByte && data[n - 1] != PdfConstants.ReverseSolidusByte)
                        {
                            leftCount++;
                            continue;
                        }

                        if (data[n] == PdfConstants.RightSquareBracketByte && data[n - 1] != PdfConstants.ReverseSolidusByte)
                        {
                            if (leftCount == 0)
                            {
                                pos = n;
                                break;
                            }

                            leftCount--;
                            continue;
                        }
                    }

                    if (pos == -1)
                    {
                        throw new InvalidDataException("No array ending boundary ']'");
                    }

                    var r = PdfArray.Parse(data[start..pos]);
                    items.Add(r);
                    i = pos + 1;

                    continue;
                }

                if (b == PdfConstants.LessThanSignByte && data[i + 1] == PdfConstants.LessThanSignByte)
                {
                    // Dictionary <<...>>
                    var pos = -1;
                    var leftCount = 0;
                    var start = i + 2;
                    for (var n = start; n < len; n++)
                    {
                        if (data[n] == PdfConstants.LessThanSignByte && data[n + 1] == PdfConstants.LessThanSignByte)
                        {
                            leftCount++;
                            // Avoid <<<< case
                            n++;
                            continue;
                        }

                        if (data[n] == PdfConstants.GreaterThanSignByte && data[n + 1] == PdfConstants.GreaterThanSignByte)
                        {
                            if (leftCount == 0)
                            {
                                pos = n;
                                break;
                            }

                            leftCount--;
                            // Avoid >>>> case
                            n++;
                            continue;
                        }
                    }

                    if (pos == -1)
                    {
                        throw new InvalidDataException("No dictionary ending boundary '>'");
                    }

                    var r = PdfDictionary.Parse(data[start..pos]);
                    items.Add(r);

                    // Pass both >>
                    i = pos + 2;

                    continue;
                }

                i++;
            }

            return items.ToArray();
        }

        /// <summary>
        /// Convert PdfArray to Rectangle
        /// 转换PDF数组为正方形数据
        /// </summary>
        /// <param name="array">Array</param>
        /// <returns>Result</returns>
        public static Rectangle? ToRectangle(this PdfArray? array)
        {
            if (array == null || array.Value.Length != 4) return null;

            var llx = (array.Value[0] as PdfInt)?.Value;
            var lly = (array.Value[1] as PdfInt)?.Value;
            var urx = (array.Value[2] as PdfInt)?.Value;
            var ury = (array.Value[3] as PdfInt)?.Value;

            if (llx == null || lly == null || urx == null || ury == null) return null;

            return new Rectangle(llx.Value, lly.Value, urx.Value - llx.Value, ury.Value - lly.Value);
        }

        /// <summary>
        /// Stream to bytes
        /// 流到字节
        /// </summary>
        /// <param name="stream">Input stream</param>
        /// <param name="bufferSize">Buffer size</param>
        /// <returns>Bytes</returns>
        public static ReadOnlyMemory<byte> ToBytes(this Stream stream, int bufferSize = 10240)
        {
            var writer = new ArrayPoolBufferWriter<byte>(bufferSize);
            int bytesRead;
            while ((bytesRead = stream.Read(writer.GetSpan(bufferSize))) > 0)
            {
                writer.Advance(bytesRead);
            }

            return writer.WrittenMemory;
        }

        /// <summary>
        /// Async stream to bytes
        /// 异步流到字节
        /// </summary>
        /// <param name="stream">Input stream</param>
        /// <param name="bufferSize">Buffer size</param>
        /// <returns>Bytes</returns>
        public static async Task<ReadOnlyMemory<byte>> ToBytesAsync(this Stream stream, int bufferSize = 10240)
        {
            var writer = new ArrayPoolBufferWriter<byte>(bufferSize);
            int bytesRead;
            while ((bytesRead = await stream.ReadAsync(writer.GetMemory(bufferSize))) > 0)
            {
                writer.Advance(bytesRead);
            }

            return writer.WrittenMemory;
        }
    }
}
