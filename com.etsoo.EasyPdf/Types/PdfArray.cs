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

        public PdfArray(params int[] values)
            : this(values.Select(v => new PdfInt(v)).ToArray())
        {
        }

        public PdfArray(params float[] values)
            : this(values.Select(v => new PdfReal(v)).ToArray())
        {
        }

        public PdfArray(params double[] values)
            : this(values.Select(v => new PdfReal(v)).ToArray())
        {
        }

        public PdfArray(params string[] values)
            : this(values.Select(v => new PdfString(v)).ToArray())
        {
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

    internal record PdfIntArray : PdfArray
    {
        public PdfIntArray(params int[] values)
            : base(values)
        {
        }
    }

    internal record PdfRealArray : PdfArray
    {
        public PdfRealArray(params float[] values)
            : base(values)
        {
        }

        public PdfRealArray(params double[] values)
            : base(values)
        {
        }
    }

    internal record PdfStringArray : PdfArray
    {
        public PdfStringArray(params string[] values)
            : base(values)
        {
        }
    }
}
