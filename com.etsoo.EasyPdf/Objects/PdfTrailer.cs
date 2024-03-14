using com.etsoo.EasyPdf.Types;

namespace com.etsoo.EasyPdf.Objects
{
    /// <summary>
    /// PDF trailer
    /// PDF 尾部信息
    /// </summary>
    internal class PdfTrailer
    {
        /// <summary>
        /// startxref value
        /// </summary>
        public int StartXref { get; }

        /// <summary>
        /// The total number of entries in the file’s cross-reference table
        /// </summary>
        public int Size { get; }

        /// <summary>
        /// The catalog dictionary for the PDF
        /// </summary>
        public PdfObject Root { get; }

        /// <summary>
        /// The document’s information dictionary
        /// </summary>
        public PdfObject? Info { get; set; }

        /// <summary>
        /// Constructor
        /// 构造函数
        /// </summary>
        /// <param name="size">Size</param>
        /// <param name="root">Root</param>
        public PdfTrailer(int startxref, int size, PdfObject root)
        {
            StartXref = startxref;
            Size=size;
            Root=root;
        }

        public async Task WriteToAsync(Stream stream)
        {
            // trailer flag
            await stream.WriteAsync(PdfConstants.TrailerBytes);

            stream.WriteByte(PdfConstants.LineFeedByte);

            // Dictionary
            var dic = new PdfDictionary(new()
            {
                [new PdfName(nameof(Size))] = new PdfInt(Size),
                [new PdfName(nameof(Root))] = Root
            });
            dic.AddNameItem(nameof(Info), Info);
            await dic.WriteToAsync(stream);

            stream.WriteByte(PdfConstants.LineFeedByte);

            // startxref flag
            await stream.WriteAsync(PdfConstants.StartXRefBytes);
            stream.WriteByte(PdfConstants.LineFeedByte);

            // value
            await new PdfInt(StartXref).WriteToAsync(stream);
            stream.WriteByte(PdfConstants.LineFeedByte);
        }
    }
}
