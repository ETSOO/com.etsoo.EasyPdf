using com.etsoo.PureIO;
using System.Text;

namespace com.etsoo.EasyPdf.Types
{
    public record PdfReference(uint Offset, ushort Generation, bool Free = false)
    {
        /// <summary>
        /// First entry in the reference table
        /// 参考表中的第一个条目
        /// </summary>
        public static readonly PdfReference FirstEntry = new(0, 65535, true);

        public static async Task<List<PdfReference>> ParseAsync(PureStreamReader reader)
        {
            var list = new List<PdfReference>();

            ushort index = 0; ushort count = 0;
            var line = await reader.ReadLineAsync();
            while (!line.Span.SequenceEqual(PdfConstants.TrailerBytes))
            {
                // Reference
                if (line.Length == 18 || line.Length == 19 && line.Span[18].Equals(PdfConstants.SpaceByte))
                {
                    if (count > 0)
                    {
                        var itemOffset = Convert.ToUInt32(Encoding.ASCII.GetString(line.Span[..10]), 10);
                        var itemGen = Convert.ToUInt16(Encoding.ASCII.GetString(line.Span.Slice(11, 5)), 10);

                        // f = 102, n = 110
                        var free = line.Span[17].Equals(102);

                        list.Add(new PdfReference(itemOffset, itemGen, free));

                        index++;
                        count--;
                    }
                }
                else
                {
                    var parts = Encoding.ASCII.GetString(line.Span).Split(' ');
                    if (parts.Length == 2)
                    {
                        if (ushort.TryParse(parts[0], out var i))
                        {
                            index = i;
                        }

                        if (ushort.TryParse(parts[1], out var c))
                        {
                            count = c;
                        }
                    }
                    else
                    {
                        throw new InvalidDataException("Invalid xref line parts");
                    }
                }

                // Next line
                line = await reader.ReadLineAsync();

                if (line.IsEmpty)
                {
                    throw new InvalidDataException("Invalid xref format");
                }
            }

            return list;
        }

        public async Task WriteToAsync(Stream stream)
        {
            var line = $"{Offset.ToString().PadLeft(10, '0')} {Generation.ToString().PadLeft(5, '0')} {(Free ? 'f' : 'n')}";
            await stream.WriteAsync(Encoding.ASCII.GetBytes(line));
            await stream.WriteAsync(new[] { PdfConstants.CarriageReturnByte, PdfConstants.LineFeedByte });
        }
    }
}
