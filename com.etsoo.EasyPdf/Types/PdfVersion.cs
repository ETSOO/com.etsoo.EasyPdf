using System.Text;

namespace com.etsoo.EasyPdf.Types
{
    public record PdfVersion
    {
        public const decimal V14 = 1.4M;
        public const decimal V15 = 1.5M;
        public const decimal V16 = 1.6M;
        public const decimal V17 = 1.7M;
        public const decimal V20 = 2.0M;

        /// <summary>
        /// Version prefix, %PDF-
        /// </summary>
        private static readonly byte[] Prefix = Encoding.ASCII.GetBytes("%PDF-");

        public static PdfVersion? Parse(ReadOnlySpan<byte> bytes)
        {
            if (bytes.StartsWith(Prefix))
            {
                var str = Encoding.ASCII.GetString(bytes[Prefix.Length..]);
                if (!decimal.TryParse(str, out var version)) version = V17;
                return new PdfVersion(version);
            }
            return null;
        }

        /// <summary>
        /// Version
        /// 版本
        /// </summary>
        public decimal Version { get; init; }

        /// <summary>
        /// Constructor
        /// 构造函数
        /// </summary>
        /// <param name="version">Version</param>
        public PdfVersion(decimal version)
        {
            Version = version;
        }

        public async Task WriteToAsync(Stream stream)
        {
            await stream.WriteAsync(Prefix);
            await stream.WriteAsync(Encoding.ASCII.GetBytes(Version.ToString("0.0")));
            stream.WriteByte(PdfConstants.LineFeedByte);
        }
    }
}
