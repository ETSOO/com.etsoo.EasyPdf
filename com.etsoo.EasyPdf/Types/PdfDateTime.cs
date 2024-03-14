using System.Text;

namespace com.etsoo.EasyPdf.Types
{
    /// <summary>
    /// PDF date time
    /// PDF 日期时间
    /// </summary>
    internal record PdfDateTime(DateTime Value) : IPdfType<DateTime>
    {
        /// <summary>
        /// Format
        /// 格式
        /// </summary>
        public const string Format = "yyyyMMddHHmmsszzz";

        /// <summary>
        /// Parse formats
        /// 解析格式
        /// </summary>
        public static readonly string[] Formats = new[] { Format, "yyyyMMddHHmmss", "yyyyMM", "yyyyMMdd", "yyyyMMddHHmm", "yyyyMMddHHmmzzz" };

        public bool KeyEquals(string item)
        {
            return false;
        }

        public async Task WriteToAsync(Stream stream)
        {
            stream.WriteByte(PdfConstants.LeftParenthesisByte);

            // Always output UTC format
            var bytes = Encoding.ASCII.GetBytes("D:" + Value.ToUniversalTime().ToString(Format).Replace(':', '\''));
            await stream.WriteAsync(bytes);

            stream.WriteByte(PdfConstants.RightParenthesisByte);
        }
    }
}
