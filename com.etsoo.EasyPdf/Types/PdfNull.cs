using System.Text;

namespace com.etsoo.EasyPdf.Types
{
    /// <summary>
    /// PDF null
    /// PDF 空值
    /// </summary>
    internal record PdfNull : IPdfType
    {
        public bool KeyEquals(string item)
        {
            return false;
        }

        public async Task WriteToAsync(Stream stream)
        {
            var bytes = Encoding.ASCII.GetBytes("null");
            await stream.WriteAsync(bytes);
        }
    }
}
