using com.etsoo.EasyPdf.Filters;
using System.Text;

namespace com.etsoo.EasyPdf.Tests
{
    [TestClass]
    public class FilterTests
    {
        [TestMethod]
        public void ASCII86Tests()
        {
            var source = @"Hello, world! 亿速思维";
            var encoded = new ASCII85Filter().Encode(Encoding.UTF8.GetBytes(source));
            Assert.AreEqual("87cURD_*#TDfTZ)+X#jW^Zg9@k('#c]<h~>", Encoding.ASCII.GetString(encoded));
            var decoded = new ASCII85Filter().Decode(encoded);
            Assert.AreEqual(source, Encoding.UTF8.GetString(decoded));
        }

        [TestMethod]
        public void LZWFilterSimpleTests()
        {
            // https://planetcalc.com/9069/ online
            var filter = new LZWFilter();
            var bytes = "-----A---B"u8.ToArray();
            var encoded = filter.Encode(bytes);

            var hex = Convert.ToHexString(encoded);
            Assert.AreEqual("800B6050220C0C8501", hex);

            var decoded = filter.Decode(encoded);
            Assert.IsTrue(decoded.SequenceEqual(bytes));
        }

        [TestMethod]
        public void LZWFilterTests()
        {
            // https://planetcalc.com/9069/ online
            var filter = new LZWFilter();
            var source = "A robot may not injure a human being or, through inaction, allow a human being to come to harm. A robot must obey the orders given to it by human beings, except where such orders would conflict with the First Law. A robot must protect its own existence.";
            var bytes = Encoding.UTF8.GetBytes(source);
            var result = Encoding.UTF8.GetString(filter.Decode(filter.Encode(bytes)));
            Assert.AreEqual(source, result);
        }

        [TestMethod]
        public void LZWFilterUtf8Tests()
        {
            // https://planetcalc.com/9069/ online
            var filter = new LZWFilter();
            var source = "神舟十三号飞行乘组指令长翟志刚是我国出舱次数最多的航天员，对于3次出舱活动，他都“感觉良好”。他说：“每一次‘感觉良好’，背后都饱含着亿万国人对我们航天事业的支持；每一次‘感觉良好’，背后都凝聚着工程全线辛勤的付出；每一次‘感觉良好’，背后都是祖国和人民对我们的托举；每一次‘感觉良好’，背后都是我们乘组之间的密切配合和个人的努力。地上训练也好、天上飞行也好，能够保持这种‘感觉良好’的状态，是因为我国载人航天事业的发展，我们的‘感觉良好’一定会继续下去。”";
            var bytes = Encoding.UTF8.GetBytes(source);
            var encoded = filter.Encode(bytes);
            var result = Encoding.UTF8.GetString(filter.Decode(encoded));
            Assert.AreEqual(source, result);
        }

        [TestMethod]
        public void ASCII85AndLZWReproduceTests()
        {
            var source =
"""
2 J 
BT
/F1 12 Tf
0 Tc 0 Tw 72.5 712 TD [ (Unencoded streams can be read easily)65 (,)] TJ
0 -14 TD [ (b)20 (ut generally tak)10 (e more space than \311)] TJ
T* (encoded streams.)Tj
0 -28 TD [ (Se)25 (v)15 (eral encoding methods are a)20 (v)25 (ailable in PDF)80 (.)] TJ
0 -14 TD (Some are used for compression and others simply)Tj
T* [ (to represent binary data in an )55 (ASCII format.)] TJ
T* (Some of the compression encoding methods are suitable )Tj
T* (for both data and images, while others are suitable only )Tj
T* (for continuous-tone images.)Tj
ET
""";
            var sourceEncoded = new LZWFilter().Encode(Encoding.ASCII.GetBytes(source));
            var sourceEncoded2 = new ASCII85Filter().Encode(sourceEncoded);

            var ascii85Bytes = new ASCII85Filter().Decode(sourceEncoded2);
            var lzwBytes = new LZWFilter().Decode(ascii85Bytes);
            var result = Encoding.UTF8.GetString(lzwBytes);

            Assert.AreEqual(source, result);
        }
    }
}
