using com.etsoo.EasyPdf.Filters;
using com.etsoo.EasyPdf.Types;
using com.etsoo.PureIO;
using CommunityToolkit.HighPerformance;
using CommunityToolkit.HighPerformance.Buffers;
using System.Buffers;
using System.Text;

namespace com.etsoo.EasyPdf.Objects
{
    internal static class PdfObjectParser
    {
        // 1 0 obj
        private static readonly byte[] objBytes = [PdfConstants.SpaceByte, 111, 98, 106];

        private static bool AreEqual(ReadOnlySpan<byte> source, ReadOnlySpan<byte> target)
        {
            var slen = source.Length;
            var tlen = target.Length;
            if (slen <= (tlen + 2) && source.StartsWith(target))
            {
                var left = source[tlen..];
                var len = left.Length;
                if (len == 0) return true;
                if (len == 1 && left[0] is PdfConstants.CarriageReturnByte or PdfConstants.LineFeedByte) return true;
                if (len == 2 && left[0] is PdfConstants.CarriageReturnByte && left[1] is PdfConstants.LineFeedByte) return true;
            }
            return false;
        }

        public static async ValueTask<PdfObjectDic?> ParseAsync(ReadOnlyMemory<byte> bytes, PureStreamReader reader)
        {
            if (bytes.Span.EndsWith(objBytes))
            {
                // obj
                var obj = PdfObject.Parse(bytes.Span, false);

                // Before endobj
                // Dictionary bytes
                var d = new ArrayPoolBufferWriter<byte>();

                // Stream bytes
                bool streamReading = false;
                ArrayPoolBufferWriter<byte>? s = null;

                while (true)
                {
                    // New line
                    var line = await reader.ReadLineAsync(streamReading ? PureStreamReadWay.ReturnAll : PureStreamReadWay.Default);
                    if (AreEqual(line.Span, PdfObject.EndobjBytes)) break;

                    // Case 1: >>stream
                    // Case 2: >> stream
                    // Case 3: >>\nstream
                    if (streamReading)
                    {
                        // endstream
                        if (AreEqual(line.Span, PdfStreamDic.endstreamBytes))
                        {
                            break;
                        }
                        else
                        {
                            s?.Write(line.Span);
                        }

                        continue;
                    }
                    else if ((streamReading = line.Span.SequenceEqual(PdfStreamDic.streamBytes)) || line.Span.EndsWith(PdfStreamDic.streamBytes))
                    {
                        if (!streamReading)
                        {
                            // >
                            var pos = line.Span.LastIndexOf(PdfConstants.GreaterThanSignByte);
                            if (pos != -1 && pos + PdfStreamDic.streamBytes.Length + 2 >= line.Span.Length)
                            {
                                d.Write(line.Span[..(pos + 1)]);

                                streamReading = true;
                                s = new ArrayPoolBufferWriter<byte>();
                            }
                        }

                        if (streamReading)
                        {
                            continue;
                        }
                    }

                    d.Write(line.Span);
                    d.Write(PdfConstants.LineFeedByte);
                }

                var dic = PdfDictionary.Parse(d.WrittenSpan[2..^2]);
                var type = dic.GetValue<string>("Type");
                if (s is not null)
                {

                    // return new PdfStreamDic(obj, dic, s.WrittenMemory[..^1])
                    var filter = dic.GetValue<string>("Filter");
                    if (filter == "FlateDecode")
                    {
                        var len = dic.GetRequiredValue<int>("Length");
                        var slen = s.WrittenSpan.Length;

                        var f = new FlateFilter();
                        var rs = Encoding.UTF8.GetString(f.Decode(s.WrittenSpan[..^1]));
                    }

                }
                else
                {
                    return type switch
                    {
                        "Pages" => new PdfPageTree(obj, dic),
                        _ => new PdfObjectDic(obj, dic)
                    };
                }
            }

            return null;
        }
    }
}
