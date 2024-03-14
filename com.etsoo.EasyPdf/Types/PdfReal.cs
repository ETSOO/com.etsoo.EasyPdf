using System.Text;

namespace com.etsoo.EasyPdf.Types
{
    /// <summary>
    /// PDF real
    /// PDF 实数
    /// </summary>
    internal record PdfReal(decimal Value) : IPdfType<decimal>
    {
        public PdfReal(float value) : this(Convert.ToDecimal(value))
        {
        }

        public PdfReal(double value) : this(Convert.ToDecimal(value))
        {
        }

        public bool KeyEquals(string item)
        {
            return false;
        }

        public async Task WriteToAsync(Stream stream)
        {
            var bytes = Encoding.ASCII.GetBytes(Value.ToString("0.0############################"));
            await stream.WriteAsync(bytes);
        }
    }
}
