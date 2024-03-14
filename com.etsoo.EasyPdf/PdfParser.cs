using com.etsoo.EasyPdf.Objects;
using com.etsoo.EasyPdf.Types;
using com.etsoo.PureIO;
using CommunityToolkit.HighPerformance.Buffers;
using System.Buffers;
using System.Text;

namespace com.etsoo.EasyPdf
{
    /// <summary>
    /// PDF file parser
    /// PDF 文件解析器
    /// </summary>
    public class PdfParser
    {
        public static async Task<PdfParser> ParseAsync(Stream pdfStream)
        {
            var parser = new PdfParser();

            await using var reader = new PureStreamReader(pdfStream);

            // First line is version
            var version = PdfVersion.Parse(reader.ReadLine()) ?? throw new InvalidDataException("No version line");
            parser.Version = version.Version;

            // Second line is binary check
            var binaryCheck = PdfBinaryCheck.Parse(reader.ReadLine()) ?? throw new InvalidDataException("No binary check line");

            // Body start
            var pos = reader.CurrentPosition;

            // Read from end
            reader.ToStreamEnd();

            // Last line maybe blank
            var lastLine = await reader.BackwardReadLineAsync();
            if (lastLine.IsEmpty) lastLine = await reader.BackwardReadLineAsync();

            // eof
            var eof = PdfEOF.Parse(lastLine.Span) ?? throw new InvalidDataException("No End of File (EOF) line");

            // startxref text and value
            var startxrefValueLine = await reader.BackwardReadLineAsync();
            var startxrefTextLine = await reader.BackwardReadLineAsync();
            var startxref = ParseStartXRef(startxrefTextLine.Span, startxrefValueLine.Span);

            // Clear
            reader.ClearBuffer();

            // xref
            reader.BaseStream.Seek(startxref, SeekOrigin.Begin);
            var xrefLine = await reader.ReadLineAsync();

            var obj = await PdfObjectParser.ParseAsync(xrefLine, reader);
            if (obj is not null)
            {
                // cross-reference stream
            }
            else if (!xrefLine.Span.SequenceEqual(PdfConstants.XrefBytes))
            {
                throw new InvalidDataException("No xref line");
            }

            // Last trailer
            await parser.ParseTrailerAsync(reader, true);

            return parser;
        }

        private static long ParseStartXRef(ReadOnlySpan<byte> startxrefTextLine, ReadOnlySpan<byte> startxrefValueLine)
        {
            if (!startxrefTextLine.SequenceEqual(PdfConstants.StartXRefBytes))
            {
                throw new InvalidDataException("No startxref line");
            }
            var startxrefValue = Encoding.ASCII.GetString(startxrefValueLine);
            if (!long.TryParse(startxrefValue, out var starxref))
            {
                throw new InvalidDataException("No startxref byte offset line");
            }
            return starxref;
        }

        /// <summary>
        /// PDF version
        /// PDF 版本
        /// </summary>
        public decimal Version { get; private set; }

        /// <summary>
        /// Private constructor, create instance use ParseAsync
        /// 私有构造函数，使用 ParseAsync 创建实例
        /// </summary>
        private PdfParser()
        {

        }

        private async Task ParseTrailerAsync(PureStreamReader reader, bool backward)
        {
            // Start from next line of xref, like "0 6" means 6 entries from 0
            // A cross-reference section may contains multiple subsections
            while (!reader.EndOfStream)
            {
                // References
                var references = await PdfReference.ParseAsync(reader);

                // Next line
                var line = await reader.ReadLineAsync();
                if (!line.Span.StartsWith(PdfConstants.DictionaryStartBytes))
                {
                    throw new InvalidDataException("Invalid trailer dictionary format");
                }

                var d = new ArrayPoolBufferWriter<byte>();
                do
                {
                    d.Write(line.Span);

                    if (line.Span.EndsWith(PdfConstants.DictionaryEndBytes))
                    {
                        var dic = PdfDictionary.Parse(d.WrittenSpan[2..^2]);

                        var entries = dic.GetRequiredValue<int>("Size");

                        var infoObj = dic.Get<PdfObject>("Info");
                        if (infoObj is not null)
                        {

                        }

                        break;
                    }

                    line = await reader.ReadLineAsync();

                } while (!line.Span.SequenceEqual(PdfConstants.StartXRefBytes) && !line.IsEmpty);

                break;
            }
        }
    }
}
