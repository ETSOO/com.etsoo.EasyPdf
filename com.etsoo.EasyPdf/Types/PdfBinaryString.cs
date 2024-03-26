using com.etsoo.EasyPdf.Filters;
using com.etsoo.EasyPdf.Support;
using System.Text;

namespace com.etsoo.EasyPdf.Types
{
    /// <summary>
    /// PDF binary string
    /// PDF 二进制字符串
    /// </summary>
    internal record PdfBinaryString : IPdfType<byte[]>
    {
        /// <summary>
        /// Bytes value
        /// 字节值
        /// </summary>
        public byte[] Value { get; }

        /// <summary>
        /// Encoding
        /// 编码
        /// </summary>
        public Encoding Encoding { get; }

        public PdfBinaryString(ReadOnlySpan<byte> bytes)
        {
            // Hexadecimal strings
            Value = bytes.FromHexCodedBytes(out var encoding).ToArray();
            Encoding = encoding;
        }

        public PdfBinaryString(byte[] bytes, Encoding encoding)
        {
            Value = bytes;
            Encoding = encoding;
        }

        public PdfBinaryString(string input, Encoding? encoding = null)
        {
            encoding ??= PdfEncoding.UTF16;
            Value = encoding.GetBytes(input);
            Encoding = encoding;
        }

        public PdfBinaryString(IEnumerable<char> chars, Encoding? encoding = null)
        {
            encoding ??= PdfEncoding.UTF16;
            Value = encoding.GetBytes(chars.ToArray());
            Encoding = encoding;
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
            await Task.CompletedTask;

            // Hexadecimal strings;
            var bytes = Value;
            if (withPreamble)
            {
                bytes = [.. Encoding.GetPreamble(), .. bytes];
            }

            stream.WriteByte(PdfConstants.LessThanSignByte);
            stream.Write(new ASCIIHexFilter().Encode(bytes));
            stream.WriteByte(PdfConstants.GreaterThanSignByte);
        }
    }
}
