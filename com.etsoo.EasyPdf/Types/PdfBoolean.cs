using System.Text;

namespace com.etsoo.EasyPdf.Types
{
    /// <summary>
    /// PDF boolean
    /// PDF 布尔值
    /// </summary>
    internal record PdfBoolean(bool Value) : IPdfType<bool>
    {
        public bool KeyEquals(string item)
        {
            return false;
        }

        public async Task WriteToAsync(Stream stream)
        {
            var bytes = Encoding.ASCII.GetBytes(Value ? "true" : "false");
            await stream.WriteAsync(bytes);
        }
    }
}
