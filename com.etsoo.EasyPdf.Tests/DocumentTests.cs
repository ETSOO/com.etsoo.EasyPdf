namespace com.etsoo.EasyPdf.Tests
{
    [TestClass]
    public class DocumentTests
    {
        [TestMethod]
        public async Task Parse()
        {
            var parser = await PdfParser.ParseAsync(File.OpenRead("Resources\\etsoo.pdf"));

            Assert.AreEqual(1.4M, parser.Version);
        }
    }
}
