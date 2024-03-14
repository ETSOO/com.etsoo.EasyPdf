namespace com.etsoo.EasyPdf.Types
{
    internal record PdfBinaryCheck
    {
        static readonly byte[] DefaultBytes = [228, 186, 191, 233];

        public static PdfBinaryCheck? Parse(ReadOnlySpan<byte> bytes)
        {
            if (bytes.Length >= 5 && bytes[0] == PdfConstants.PercentSignByte && bytes[1..].IsBinaryCheck())
            {
                return new PdfBinaryCheck(bytes[1..].ToArray());
            }
            return null;
        }

        /// <summary>
        /// Bytes
        /// 字节
        /// </summary>
        public byte[] Bytes { get; init; }

        /// <summary>
        /// Constructor
        /// 构造函数
        /// </summary>
        /// <param name="bytes">Bytes</param>
        public PdfBinaryCheck() : this(DefaultBytes)
        {
        }

        /// <summary>
        /// Constructor
        /// 构造函数
        /// </summary>
        /// <param name="bytes">Bytes</param>
        public PdfBinaryCheck(byte[] bytes)
        {
            Bytes = bytes;
        }

        public async Task WriteToAsync(Stream stream)
        {
            stream.WriteByte(PdfConstants.PercentSignByte);
            await stream.WriteAsync(Bytes);
            stream.WriteByte(PdfConstants.LineFeedByte);
        }
    }
}
