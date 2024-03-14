using System.Text;

namespace com.etsoo.EasyPdf.Support
{
    /// <summary>
    /// PDF text string encoding
    /// https://www.pdfa.org/understanding-utf-8-in-pdf-2-0/
    /// PDF 文本字符串编码
    /// </summary>
    public static class PdfEncoding
    {
        /// <summary>
        /// UTF8 with leading byte order markers (BOM) 239, 187, 191
        /// </summary>
        public static readonly Encoding UTF8 = Encoding.UTF8;

        /// <summary>
        /// UTF16BE with leading byte order markers (BOM) 254, 255
        /// </summary>
        public static readonly Encoding UTF16 = new UnicodeEncoding(true, true, true);

        /// <summary>
        /// PdfDocEncoding
        /// </summary>
        public static readonly Encoding PdfDoc = new PdfDocEncoding();

        /// <summary>
        /// Detect encoding
        /// 探测编码
        /// </summary>
        /// <param name="bytes">Bytes</param>
        /// <returns>Encoding, BOM count</returns>
        public static (Encoding, byte) Detect(ReadOnlySpan<byte> bytes)
        {
            var len = bytes.Length;

            if (len > 2 && bytes[0] == 254 && bytes[1] == 255)
            {
                // Two bytes (U+FEFF) represent the Unicode byte order marker
                return (UTF16, 2);
            }

            if (len > 3 && bytes[0] == 239 && bytes[1] == 187 && bytes[2] == 191)
            {
                // 3-byte UTF-8 BOM (EF BB BF)
                return (UTF8, 3);
            }

            // PDFDocEncoding
            return (PdfDoc, 0);
        }

        /// <summary>
        /// Detect encoding with Hex form
        /// 16进制字符探测编码
        /// </summary>
        /// <param name="bytes">Bytes</param>
        /// <returns>Encoding, BOM count</returns>
        public static (Encoding, byte) DetectHex(ReadOnlySpan<byte> bytes)
        {
            if (bytes.StartsWith("FEFF"u8))
            {
                // Two bytes (U+FEFF) represent the Unicode byte order marker
                return (UTF16, 4);
            }

            if (bytes.StartsWith("EFBBBF"u8))
            {
                // 3-byte UTF-8 BOM (EF BB BF)
                return (UTF8, 6);
            }

            // PDFDocEncoding
            return (PdfDoc, 0);
        }
    }
}
