using com.etsoo.EasyPdf.Filters;
using com.etsoo.EasyPdf.Support;
using System.Text;

namespace com.etsoo.EasyPdf.Types
{
    /// <summary>
    /// PDF binary string
    /// PDF 二进制字符串
    /// </summary>
    internal record PdfBinaryString(string Value, Encoding? TextEncoding = null) : IPdfType<string>
    {
        public static PdfBinaryString Parse(ReadOnlySpan<byte> bytes)
        {
            // Hexadecimal strings
            var value = bytes.FromHexCoded(out var encoding);
            return new PdfBinaryString(value, encoding);
        }

        public bool KeyEquals(string item)
        {
            return Value.Equals(item);
        }

        public async Task WriteToAsync(Stream stream)
        {
            await WriteToAsync(stream, true);
        }

        public async Task WriteToAsync(Stream stream, bool withPreamble)
        {
            // Encoding
            var encoding = TextEncoding ?? PdfEncoding.UTF16;

            await Task.CompletedTask;

            // Hexadecimal strings;
            var bytes = encoding.GetBytes(Value);
            if (withPreamble)
            {
                bytes = [.. encoding.GetPreamble(), .. bytes];
            }

            stream.WriteByte(PdfConstants.LessThanSignByte);
            stream.Write(new ASCIIHexFilter().Encode(bytes));
            stream.WriteByte(PdfConstants.GreaterThanSignByte);
        }
    }
}
