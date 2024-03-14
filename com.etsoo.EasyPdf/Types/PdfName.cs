using CommunityToolkit.HighPerformance;
using CommunityToolkit.HighPerformance.Buffers;
using System.Buffers;
using System.Text;
using System.Text.RegularExpressions;

namespace com.etsoo.EasyPdf.Types
{
    /// <summary>
    /// PDF literal name
    /// PDF 名称
    /// </summary>
    internal partial record PdfName(string Value) : IPdfType<string>
    {
        public static PdfName Parse(ReadOnlySpan<byte> bytes)
        {
            var str = Encoding.UTF8.GetString(bytes);
            var value = MyRegex().Replace(str, new MatchEvaluator((match) =>
            {
                var b = Convert.ToByte(match.Groups[1].Value, 16);
                return Encoding.UTF8.GetString(new[] { b });
            }));
            return new PdfName(value);
        }
        public bool KeyEquals(string item)
        {
            return Value.Equals(item);
        }

        public async Task WriteToAsync(Stream stream)
        {
            stream.WriteByte(PdfConstants.SolidusByte);

            var w = new ArrayPoolBufferWriter<byte>();
            foreach (var b in Encoding.UTF8.GetBytes(Value))
            {
                if (b < 33 || b > 126 || b is PdfConstants.NumberSignByte or PdfConstants.SpaceByte || PdfConstants.DelimiterCharacters.Contains(b))
                {
                    var str = Convert.ToString(b, 16);
                    w.Write(Encoding.ASCII.GetBytes($"#{str}"));
                    continue;
                }

                w.Write(b);
            }

            await stream.WriteAsync(w.WrittenMemory);
        }

        [GeneratedRegex("#([0-9a-z]{2})")]
        private static partial Regex MyRegex();
    }
}
