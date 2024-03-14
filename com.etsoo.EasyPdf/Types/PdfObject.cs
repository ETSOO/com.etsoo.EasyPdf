using System.Text;

namespace com.etsoo.EasyPdf.Types
{
    /// <summary>
    /// PDF object
    /// PDF 对象
    /// </summary>
    public record PdfObject(ushort Value, bool Reference) : IPdfType<ushort>
    {
        /// <summary>
        /// endobj bytes
        /// </summary>
        public static readonly byte[] EndobjBytes = Encoding.ASCII.GetBytes("endobj");

        public static PdfObject Parse(ReadOnlySpan<byte> bytes, bool reference)
        {
            var line = Encoding.ASCII.GetString(bytes);
            var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            if (!ushort.TryParse(parts[0], out var value))
            {
                value = 0;
            }

            if (!ushort.TryParse(parts[1], out var generation))
            {
                generation = 0;
            }

            return new PdfObject(value, reference)
            {
                Generation = generation
            };
        }

        /// <summary>
        /// Generation for update count
        /// 更新代数
        /// </summary>
        public uint Generation { get; set; }

        /// <summary>
        /// Create a reference object
        /// 创建一个索引对象
        /// </summary>
        /// <returns>Result</returns>
        public PdfObject AsRef()
        {
            return this with { Reference = true };
        }

        public bool KeyEquals(string item)
        {
            return false;
        }

        public async Task WriteToAsync(Stream stream)
        {
            var key = Reference ? "R" : "obj";
            var str = $"{Value} {Generation} {key}";
            await stream.WriteAsync(Encoding.ASCII.GetBytes(str));
        }

        public async Task WriteEndToAsync(Stream stream)
        {
            await stream.WriteAsync(EndobjBytes);
        }
    }
}
