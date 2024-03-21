using com.etsoo.EasyPdf.Filters;
using com.etsoo.EasyPdf.Types;
using System.Text;

namespace com.etsoo.EasyPdf.Objects
{
    /// <summary>
    /// PDF stream filter
    /// PDF 流过滤器
    /// </summary>
    public enum PdfStreamFilter
    {
        None,
        FlateDecode
    }

    /// <summary>
    /// PDF stream dictionary
    /// PDF 流字典
    /// </summary>
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
        public PdfStreamFilter? Filter { get; set; }

        /// <summary>
        /// Constructor
        /// 构造函数
        /// </summary>
        /// <param name="bytes">Stream bytes</param>
        public PdfStreamDic(ReadOnlyMemory<byte> bytes) : base()
        {
            Bytes = bytes;

            // Add the length property
            Dic.AddNameItem("Length", new PdfInt(Bytes.Length));

            // Set the default filter
            Filter ??= (PdfDocument.Debug ? PdfStreamFilter.None : PdfStreamFilter.FlateDecode);
        }

        /// <summary>
        /// Constructor
        /// 构造函数
        /// </summary>
        /// <param name="stream">Data stream</param>
        public PdfStreamDic(Stream stream) : this(stream.ToBytes())
        {

        }

        /// <summary>
        /// Constructor
        /// 构造函数
        /// </summary>
        /// <param name="obj">Object</param>
        /// <param name="bytes">Stream bytes</param>
        public PdfStreamDic(PdfObject obj, ReadOnlyMemory<byte> bytes) : base(obj)
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
