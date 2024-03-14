using com.etsoo.EasyPdf.Types;
using com.etsoo.PureIO;
using System.Text;

namespace com.etsoo.EasyPdf.Tests
{
    [TestClass]
    public class TypeTests
    {
        [TestMethod]
        public async Task NumberTests()
        {
            using var stream = new MemoryStream(Encoding.ASCII.GetBytes("+17 -9.2 5532."));
            using var reader = new PureStreamReader(stream);

            var r1 = PdfArray.Parse(reader.ReadLine());
            Assert.AreEqual(3, r1.Value.Length);

            Assert.AreEqual(17, (r1.Value[0] as PdfInt)?.Value);
            Assert.AreEqual(-9.2M, (r1.Value[1] as PdfReal)?.Value);
            Assert.AreEqual(5532M, (r1.Value[2] as PdfReal)?.Value);

            using var ws = new MemoryStream();
            await r1.WriteToAsync(ws);
            Assert.AreEqual("[17 -9.2 5532.0]", Encoding.ASCII.GetString(ws.ToArray()));
        }

        [TestMethod]
        public async Task Utf8StringTests()
        {
            var bs = new PdfBinaryString("Blank 空白模板", Encoding.UTF8);
            using var ws = new MemoryStream();
            await bs.WriteToAsync(ws);
            Assert.AreEqual($"<EFBBBF426C616E6B20E7A9BAE799BDE6A8A1E69DBF>", Encoding.ASCII.GetString(ws.ToArray()));
        }

        [TestMethod]
        public void StringWithMultipleLinesTests()
        {
            var bytes = Encoding.ASCII.GetBytes(@"These \
two strings \
are the same.");

            var r1 = PdfString.Parse(bytes) as PdfString;
            Assert.AreEqual("These two strings are the same.", r1?.Value);
        }

        [TestMethod]
        public void StringLineTests()
        {
            using var stream = new MemoryStream(Encoding.ASCII.GetBytes("So does this one.\\n"));
            using var reader = new PureStreamReader(stream);

            var r1 = PdfString.Parse(reader.ReadLine()) as PdfString;
            Assert.AreEqual("So does this one.\n", r1?.Value);
        }

        [TestMethod]
        public void StringOctalTests()
        {
            using var stream = new MemoryStream(Encoding.ASCII.GetBytes("\\53\\245"));
            using var reader = new PureStreamReader(stream);

            var r1 = PdfString.Parse(reader.ReadLine()) as PdfString;
            Assert.AreEqual("+�", r1?.Value);
        }

        [TestMethod]
        public async Task StringDateTimeTests()
        {
            using var stream = new MemoryStream(Encoding.ASCII.GetBytes("D:199812231952-08'00"));
            using var reader = new PureStreamReader(stream);

            var r1 = PdfString.Parse(reader.ReadLine()) as PdfDateTime;
            Assert.AreEqual(12, r1?.Value.Month);

            if (r1 is not null)
            {
                using var ws = new MemoryStream();
                await r1.WriteToAsync(ws);
                Assert.AreEqual("(D:19981224035200+00'00)", Encoding.ASCII.GetString(ws.ToArray()));
            }
        }

        [TestMethod]
        public async Task NameTests()
        {
            var source = "paired#28#20#29parentheses#23";
            using var stream = new MemoryStream(Encoding.ASCII.GetBytes(source));
            using var reader = new PureStreamReader(stream);

            var r1 = PdfName.Parse(reader.ReadLine());
            Assert.AreEqual("paired( )parentheses#", r1.Value);

            using var ws = new MemoryStream();
            await r1.WriteToAsync(ws);
            Assert.AreEqual($"/{source}", Encoding.ASCII.GetString(ws.ToArray()));
        }

        [TestMethod]
        public async Task ArrayTests()
        {
            var source = "549 3.14 false (Ralph) /SomeName";
            using var stream = new MemoryStream(Encoding.ASCII.GetBytes(source));
            using var reader = new PureStreamReader(stream);

            var r1 = PdfArray.Parse(reader.ReadLine());
            Assert.AreEqual(5, r1.Value.Length);

            using var ws = new MemoryStream();
            await r1.WriteToAsync(ws);
            Assert.AreEqual($"[{source}]", Encoding.ASCII.GetString(ws.ToArray()));
        }

        [TestMethod]
        public async Task ArrayObjectsTests()
        {
            var source = " 3 0 R 19 0 R 28 0 R 41 0 R 48 0 R 52 0 R 60 0 R 68 0 R 72 0 R 74 0 R";
            using var stream = new MemoryStream(Encoding.ASCII.GetBytes(source));
            using var reader = new PureStreamReader(stream);

            var r1 = PdfArray.Parse(reader.ReadLine());
            Assert.AreEqual(10, r1.Value.Length);

            using var ws = new MemoryStream();
            await r1.WriteToAsync(ws);
            Assert.AreEqual("[3 0 R 19 0 R 28 0 R 41 0 R 48 0 R 52 0 R 60 0 R 68 0 R 72 0 R 74 0 R]", Encoding.ASCII.GetString(ws.ToArray()));
        }

        [TestMethod]
        public async Task ObjectTests()
        {
            var source = @"/Type /Example
/Subtype /DictionaryExample
/Version 0.01
/IntegerItem 12
/StringItem (a string)
/Subdictionary << /Item1 0.4
        /Item2 true
        /LastItem (not!)
        /VeryLastItem (OK)
    >>";
            var bytes = Encoding.ASCII.GetBytes(source);
            var r1 = PdfDictionary.Parse(bytes);
            Assert.AreEqual(6, r1.Value.Keys.Count);

            using var ws = new MemoryStream();
            await r1.WriteToAsync(ws);
            Assert.AreEqual("<</Type /Example\n/Subtype /DictionaryExample\n/Version 0.01\n/IntegerItem 12\n/StringItem (a string)\n/Subdictionary <</Item1 0.4\n/Item2 true\n/LastItem (not!)\n/VeryLastItem (OK)>>>>", Encoding.ASCII.GetString(ws.ToArray()));
        }

        [TestMethod]
        public async Task ObjectCompactTests()
        {
            var source = @"/Type/Page/Parent 2 0 R/Resources<</ExtGState<</GS5 5 0 R/GS10 10 0 R>>/XObject<</Image6 6 0 R/Image11 11 0 R/Image13 13 0 R/Image15 15 0 R>>/Font<</F1 8 0 R/F2 17 0 R>>/ProcSet[/PDF/Text/ImageB/ImageC/ImageI] >>/MediaBox[ 0 0 1920 1080] /Contents 4 0 R/Group<</Type/Group/S/Transparency/CS/DeviceRGB>>/Tabs/S/StructParents 0";
            var bytes = Encoding.ASCII.GetBytes(source);
            var r1 = PdfDictionary.Parse(bytes);
            Assert.AreEqual(8, r1.Value.Keys.Count);

            using var ws = new MemoryStream();
            await r1.WriteToAsync(ws);
            Assert.AreEqual("<</Type /Page\n/Parent 2 0 R\n/Resources <</ExtGState <</GS5 5 0 R\n/GS10 10 0 R>>\n/XObject <</Image6 6 0 R\n/Image11 11 0 R\n/Image13 13 0 R\n/Image15 15 0 R>>\n/Font <</F1 8 0 R\n/F2 17 0 R>>\n/ProcSet [/PDF /Text /ImageB /ImageC /ImageI]>>\n/MediaBox [0 0 1920 1080]\n/Contents 4 0 R\n/Group <</Type /Group\n/S /Transparency\n/CS /DeviceRGB>>\n/Tabs /S\n/StructParents 0>>", Encoding.ASCII.GetString(ws.ToArray()));
        }
    }
}
