using System.Text;

namespace com.etsoo.EasyPdf.Types
{
    internal record PdfEnum<E>(E Value) : IPdfType<E> where E : Enum
    {
        public bool KeyEquals(string item)
        {
            return Value.ToString().Equals(item);
        }

        public async Task WriteToAsync(Stream stream)
        {
            stream.WriteByte(PdfConstants.SolidusByte);
            await stream.WriteAsync(Encoding.ASCII.GetBytes(Value.ToString()));
        }
    }
}
