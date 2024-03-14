namespace com.etsoo.EasyPdf.Types
{
    /// <summary>
    /// PDF array
    /// PDF 数组
    /// </summary>
    internal record PdfArray(IPdfType[] Value) : IPdfType<IPdfType[]>
    {
        public static PdfArray Parse(ReadOnlySpan<byte> bytes)
        {
            return new PdfArray(bytes.Parse());
        }

        public bool KeyEquals(string item)
        {
            return false;
        }

        public async Task WriteToAsync(Stream stream)
        {
            stream.WriteByte(PdfConstants.LeftSquareBracketByte);

            for (var i = 0; i < Value.Length; i++)
            {
                if (i > 0)
                {
                    stream.WriteByte(PdfConstants.SpaceByte);
                }
                await Value[i].WriteToAsync(stream);
            }

            stream.WriteByte(PdfConstants.RightSquareBracketByte);
        }
    }
}
