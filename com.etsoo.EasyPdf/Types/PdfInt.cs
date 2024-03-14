using System.Text;

namespace com.etsoo.EasyPdf.Types
{
    /// <summary>
    /// PDF integer
    /// PDF 整数
    /// </summary>
    internal record PdfInt(int Value) : IPdfType<int>
    {
        public static IPdfType Parse(ReadOnlySpan<byte> bytes)
        {
            var str = Encoding.ASCII.GetString(bytes);
            if (str.Contains('.') && decimal.TryParse(str, out var d))
            {
                // Real
                return new PdfReal(d);
            }
            else
            {
                if (!int.TryParse(str, out var i)) i = 0;
                return new PdfInt(i);
            }
        }

        public bool KeyEquals(string item)
        {
            return false;
        }

        public async Task WriteToAsync(Stream stream)
        {
            var bytes = Encoding.ASCII.GetBytes(Value.ToString());
            await stream.WriteAsync(bytes);
        }
    }
}
