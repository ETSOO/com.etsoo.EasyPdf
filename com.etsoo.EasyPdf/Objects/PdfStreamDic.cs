using com.etsoo.EasyPdf.Filters;
using com.etsoo.EasyPdf.Types;
using System.Text;

namespace com.etsoo.EasyPdf.Objects
{
    public enum PdfStreamFilter
    {
        None,
        FlateDecode
    }

    public class PdfStreamDic : PdfObjectDic
    {
        /// <summary>
        /// stream bytes
        /// </summary>
        public static readonly byte[] streamBytes = Encoding.ASCII.GetBytes("stream");

        /// <summary>
        /// endstream bytes
        /// </summary>
        public static readonly byte[] endstreamBytes = Encoding.ASCII.GetBytes("endstream");

        /// <summary>
        /// Stream bytes
        /// 流字节
        /// </summary>
        protected ReadOnlyMemory<byte> Bytes { get; }

        /// <summary>
        /// Encode/decode filter
        /// 编码/解码过滤器
        /// </summary>
        public PdfStreamFilter Filter { get; init; } = PdfStreamFilter.None;

        /// <summary>
        /// Constructor
        /// 构造函数
        /// </summary>
        /// <param name="bytes">Stream bytes</param>
        /// <param name="dic">Dictionary data</param>
        public PdfStreamDic(ReadOnlyMemory<byte> bytes, PdfDictionary? dic) : base(null, dic)
        {
            Bytes = bytes;

            // Add the length property
            Dic.AddNameItem("Length", new PdfInt(Bytes.Length));
        }

        /// <summary>
        /// Constructor
        /// 构造函数
        /// </summary>
        /// <param name="stream">Data stream</param>
        /// <param name="dic">Dictionary data</param>
        public PdfStreamDic(Stream stream, PdfDictionary? dic) : this(stream.ToBytes(), dic)
        {

        }

        /// <summary>
        /// Constructor
        /// 构造函数
        /// </summary>
        /// <param name="obj">Object</param>
        /// <param name="dic">Dictionary data</param>
        /// <param name="bytes">Stream bytes</param>
        public PdfStreamDic(PdfObject obj, PdfDictionary? dic, ReadOnlyMemory<byte> bytes) : base(obj, dic)
        {
            Bytes = bytes;
        }

        protected override void AddItems()
        {
            base.AddItems();

            if (Filter != PdfStreamFilter.None)
                Dic.AddNames(nameof(Filter), Filter.ToString());
        }

        protected override async Task WriteOthersAsync(Stream stream)
        {
            // Add a white-space
            stream.WriteByte(PdfConstants.SpaceByte);

            // stream
            await stream.WriteAsync(streamBytes);
            stream.WriteByte(PdfConstants.LineFeedByte);

            if (Filter == PdfStreamFilter.FlateDecode)
            {
                var filter = new FlateFilter();
                await using var result = await filter.EncodeAsync(Bytes);
                result.Position = 0;
                await result.CopyToAsync(stream);
            }
            else
            {
                await stream.WriteAsync(Bytes);
            }

            stream.WriteByte(PdfConstants.LineFeedByte);
            await stream.WriteAsync(endstreamBytes);
        }
    }
}
