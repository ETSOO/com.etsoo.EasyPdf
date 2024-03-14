using com.etsoo.EasyPdf.Support;
using System.Text;

namespace com.etsoo.EasyPdf.Tests
{
    [TestClass]
    public class ExtensionTests
    {
        [TestMethod]
        public void StringToHexBytes()
        {
            var encoding = PdfEncoding.UTF16;
            var bytes = encoding.GetPreamble().Concat(encoding.GetBytes("Blank 空白模板")).ToArray();
            Assert.AreEqual("FEFF0042006C0061006E006B00207A7A767D6A21677F", Convert.ToHexString(bytes));
        }

        [TestMethod]
        public void StringFromHexCoded()
        {
            ReadOnlySpan<byte> span = Encoding.ASCII.GetBytes("4E6F762073686D6F7A206B6120706F702E");
            var result = span.FromHexCoded(out var encoding);
            Assert.AreEqual("Nov shmoz ka pop.", result);
            Assert.AreEqual("pdf", encoding.WebName);
        }

        [TestMethod]
        public void UtfStringFromHexCoded()
        {
            ReadOnlySpan<byte> span = Encoding.ASCII.GetBytes("FEFF0042006C0061006E006B00207A7A767D6A21677F");
            var result = span.FromHexCoded(out var encoding);
            Assert.AreEqual("Blank 空白模板", result);
            Assert.AreEqual("utf-16BE", encoding.WebName);
        }

        [TestMethod]
        public void Utf8StringFromHexCoded()
        {
            ReadOnlySpan<byte> span = Encoding.ASCII.GetBytes("EFBBBF426C616E6B20E7A9BAE799BDE6A8A1E69DBF");
            var result = span.FromHexCoded(out var encoding);
            Assert.AreEqual("Blank 空白模板", result);
            Assert.AreEqual("utf-8", encoding.WebName);
        }
    }
}
