using com.etsoo.PureIO;
using com.etsoo.Utils;
using SixLabors.ImageSharp.Formats.Png;
using System.Buffers;
using System.IO.Compression;

namespace com.etsoo.EasyPdf.Content
{
    /// <summary>
    /// PDF PNG image
    /// http://www.libpng.org/pub/png/spec/1.2/PNG-Contents.html
    /// </summary>
    internal class PdfImagePng
    {
        /// <summary>
        /// PNG file signature
        /// PNG 文件签名
        /// </summary>
        public static readonly byte[] signature = [137, 80, 78, 71, 13, 10, 26, 10];

        /// <summary>
        /// Image Header, must be first
        /// </summary>
        static ReadOnlySpan<byte> IHDR => "IHDR"u8;

        /// <summary>
        /// Image data, multiple IDATs must be consecutive
        /// </summary>
        static ReadOnlySpan<byte> IDAT => "IDAT"u8;

        /// <summary>
        /// End of the image, must be last
        /// </summary>
        static ReadOnlySpan<byte> IEND => "IEND"u8;

        /// <summary>
        /// Width
        /// 宽度
        /// </summary>
        public int Width { get; init; }

        /// <summary>
        /// Height
        /// 高度
        /// </summary>
        public int Height { get; init; }

        /// <summary>
        /// Bit depth
        /// 位深度
        /// </summary>
        public byte BitDepth { get; init; }

        /// <summary>
        /// Color type
        /// 颜色类型
        /// </summary>
        public readonly PngColorType ColorType = PngColorType.RgbWithAlpha;

        /// <summary>
        /// Compression method
        /// 压缩方法
        /// </summary>
        public readonly byte CompressionMethod = 0;

        /// <summary>
        /// Filter method
        /// 过滤方式
        /// </summary>
        public readonly byte FilterMethod = 0;

        /// <summary>
        /// Interlace method
        /// 隔行扫描法
        /// </summary>
        public readonly byte InterlaceMethod = 0;

        /// <summary>
        /// Raw data
        /// 原始数据
        /// </summary>
        public ReadOnlyMemory<byte> Data { get; init; }

        /// <summary>
        /// Parse PNG image
        /// 解析 PNG 图片
        /// </summary>
        /// <param name="stream">Image stream</param>
        /// <returns>Result</returns>
        public static async Task<PdfImagePng> ParseAsync(Stream stream)
        {
            // Reader
            await using var sr = new PureStreamReader(stream);

            // Signature check
            if (!sr.ReadBytes(signature.Length).SequenceEqual(signature))
            {
                throw new Exception("Invalid PNG file");
            }

            // Data writer
            var dw = new ArrayBufferWriter<byte>();

            var width = 0;
            var height = 0;
            byte bitDepth = 0;
            PngColorType colorType = PngColorType.RgbWithAlpha;
            byte compressionMethod = 0;
            byte filterMethod = 0;
            byte interlaceMethod = 0;

            // Parse chunks
            while (true)
            {
                // Length
                var length = sr.ReadUint();

                // Chunk Type
                var type = sr.ReadBytes(4).ToArray();

                // Chunk Data
                if (IHDR.SequenceEqual(type))
                {
                    // http://www.libpng.org/pub/png/spec/1.2/PNG-Chunks.html#C.IHDR

                    // Width
                    width = sr.ReadInt();

                    // Height
                    height = sr.ReadInt();

                    // Bit depth
                    bitDepth = sr.ReadOne();

                    // Color type
                    colorType = (PngColorType)sr.ReadOne();

                    // Compression method
                    compressionMethod = sr.ReadOne();

                    // Filter method
                    filterMethod = sr.ReadOne();

                    // Interlace method
                    interlaceMethod = sr.ReadOne();
                }
                else if (IDAT.SequenceEqual(type))
                {
                    // Image data
                    // Multiple IDATs must be consecutive
                    dw.Write(sr.ReadBytes(length));
                }
                else if (IEND.SequenceEqual(type))
                {
                    break;
                }
                else
                {
                    // No interest
                    sr.Discard(length);
                }

                // CRC
                sr.Skip(4);
            }

            // Validate
            if (width < 1 || height < 0 || dw.WrittenCount == 0)
            {
                throw new Exception("Invalid PNG size");
            }

            if (colorType != PngColorType.RgbWithAlpha || compressionMethod != 0 || filterMethod != 0 || interlaceMethod != 0)
            {
                throw new Exception("Only support RGBA without filter and interlance PNG");
            }

            return new PdfImagePng
            {
                Width = width,
                Height = height,
                BitDepth = bitDepth,
                Data = dw.WrittenMemory
            };
        }

        private static int[] GetPixels(byte[] bytes, byte bitDepth)
        {
            var len = bytes.Length;
            switch (bitDepth)
            {
                case 8:
                    {
                        var res = new int[len];
                        for (var k = 0; k < res.Length; ++k)
                            res[k] = bytes[k] & 0xff;
                        return res;
                    }
                case 16:
                    {
                        var res = new int[len / 2];
                        for (var k = 0; k < res.Length; ++k)
                            res[k] = ((bytes[k * 2] & 0xff) << 8) + (bytes[k * 2 + 1] & 0xff);
                        return res;
                    }
                default:
                    {
                        var res = new int[len * 8 / bitDepth];
                        var idx = 0;
                        var passes = 8 / bitDepth;
                        var mask = (1 << bitDepth) - 1;
                        for (var k = 0; k < len; ++k)
                        {
                            for (var j = passes - 1; j >= 0; --j)
                            {
                                res[idx++] = (bytes[k] >>> (bitDepth * j)) & mask;
                            }
                        }
                        return res;
                    }
            }
        }

        private static void SetPixel(byte[] image, int[] data, int offset, int size, int x, int y, byte bitDepth, int bytesPerRow)
        {
            if (bitDepth == 8)
            {
                var pos = bytesPerRow * y + size * x;
                for (var k = 0; k < size; ++k)
                    image[pos + k] = (byte)data[k + offset];
            }
            else if (bitDepth == 16)
            {
                var pos = bytesPerRow * y + size * x;
                for (var k = 0; k < size; ++k)
                    image[pos + k] = (byte)(data[k + offset] >>> 8);
            }
            else
            {
                var pos = bytesPerRow * y + x / (8 / bitDepth);
                var v = data[offset] << (8 - bitDepth * (x % (8 / bitDepth)) - bitDepth);
                image[pos] |= (byte)v;
            }
        }

        private async Task<Stream> DecodeDataAsync()
        {
            await using var zip = new ZLibStream(SharedUtils.GetStream(Data.ToArray()), CompressionMode.Decompress);
            var stream = PdfConstants.StreamManager.GetStream();
            await zip.CopyToAsync(stream);
            stream.Position = 0;
            return stream;
        }

        public async Task<(byte[] image, byte[] mask)> BuildPdfDataAsync()
        {
            var image = new byte[Width * 3 * Height];
            var mask = new byte[Width * Height];

            await using var data = await DecodeDataAsync();

            // Calculated by color type
            var inputBands = 4;
            var sizes = 3;

            var bytesPerRow = (inputBands * Width * BitDepth + 7) / 8;

            var row = new byte[bytesPerRow];

            for (var y = 0; y < Height; y++)
            {
                var filter = data.ReadByte();
                if (filter == -1) break;

                await data.ReadExactlyAsync(row);

                var pixels = GetPixels(row, BitDepth);

                var yStride = (sizes * Width * (BitDepth == 16 ? 8 : BitDepth) + 7) / 8;

                for (var x = 0; x < Width; x++)
                {
                    var offset = inputBands * x;
                    SetPixel(image, pixels, offset, sizes, x, y, BitDepth, yStride);
                }

                if (BitDepth == 16)
                {
                    for (var k = 0; k < Width; ++k)
                    {
                        pixels[k * inputBands + sizes] >>>= 8;
                    }
                }

                for (var x = 0; x < Width; x++)
                {
                    var offset = inputBands * x + sizes;
                    SetPixel(mask, pixels, offset, 1, x, y, 8, Width);
                }
            }

            return (image, mask);
        }
    }
}
