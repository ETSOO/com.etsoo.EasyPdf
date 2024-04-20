using com.etsoo.EasyPdf.Filters;
using com.etsoo.EasyPdf.Types;
using System.Text;

namespace com.etsoo.EasyPdf.Objects
{
    /// <summary>
    /// PDF stream dictionary
    /// PDF 流字典
    /// </summary>
    internal class PdfStreamDic : PdfObjectDic
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
        protected ReadOnlyMemory<byte> Bytes { get; private set; }

        /// <summary>
        /// Encode/decode filters
        /// 编码/解码过滤器
        /// </summary>
        public IFilter[]? Filters { get; set; }

        /// <summary>
        /// Constructor
        /// 构造函数
        /// </summary>
        /// <param name="bytes">Stream bytes</param>
        public PdfStreamDic(ReadOnlyMemory<byte> bytes) : base()
        {
            Bytes = bytes;
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

        protected override async Task AddItemsAsync()
        {
            await base.AddItemsAsync();

            // Set the default filter
            if (!PdfDocument.Debug)
            {
                Filters ??= [new FlateFilter()];
            }

            if (Filters?.Length > 0)
            {
                if (Filters.Length == 1)
                {
                    Dic.AddNames("Filter", Filters[0].Name);
                }
                else
                {
                    Dic.AddNameArray("Filter", Filters.Select(f => new PdfName(f.Name)));
                }

                foreach (var filter in Filters)
                {
                    Bytes = await filter.EncodeAsync(Bytes);
                }
            }

            // Add the length property
            Dic.AddNameItem("Length", new PdfInt(Bytes.Length));
        }

        protected override async Task WriteOthersAsync(Stream stream)
        {
            // Add a white-space
            stream.WriteByte(PdfConstants.SpaceByte);

            // stream
            await stream.WriteAsync(streamBytes);
            stream.WriteByte(PdfConstants.LineFeedByte);

            // Write bytes to the stream
            await stream.WriteAsync(Bytes);

            stream.WriteByte(PdfConstants.LineFeedByte);
            await stream.WriteAsync(endstreamBytes);
        }
    }
}
