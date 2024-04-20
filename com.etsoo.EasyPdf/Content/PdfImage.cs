using com.etsoo.EasyPdf.Filters;
using com.etsoo.EasyPdf.Objects;
using com.etsoo.EasyPdf.Support;
using com.etsoo.EasyPdf.Types;
using com.etsoo.Utils;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Processing;

namespace com.etsoo.EasyPdf.Content
{
    /// <summary>
    /// PDF image
    /// PDF 图片
    /// </summary>
    public class PdfImage : PdfChunk
    {
        private static readonly IImageFormat[] SupportedFormats = [
            JpegFormat.Instance,
            PngFormat.Instance
        ];

        /// <summary>
        /// Load image from path
        /// 从路径加载图片
        /// </summary>
        /// <param name="path">Image path</param>
        /// <param name="cancellationToken">Cacellation token</param>
        /// <returns>Result</returns>
        public static async Task<PdfImage> LoadAsync(string path, CancellationToken cancellationToken = default)
        {
            var image = await Image.LoadAsync(path, cancellationToken);
            return new PdfImage(image);
        }

        /// <summary>
        /// Load image from stream
        /// 从流加载图片
        /// </summary>
        /// <param name="stream">Image stream</param>
        /// <param name="cancellationToken">Cacellation token</param>
        /// <returns>Result</returns>
        public static async Task<PdfImage> LoadAsync(Stream stream, CancellationToken cancellationToken = default)
        {
            var image = await Image.LoadAsync(stream, cancellationToken);
            return new PdfImage(image);
        }

        private static async Task<ReadOnlyMemory<byte>> ReadBytesAsync(Image image, IImageEncoder encoder)
        {
            await using var stream = await ReadStreamAsync(image, encoder);
            return await stream.ToBytesAsync();
        }

        private static async Task<Stream> ReadStreamAsync(Image image, IImageEncoder encoder)
        {
            var stream = SharedUtils.GetStream();
            await image.SaveAsync(stream, encoder);
            stream.Position = 0;

            return stream;
        }

        private int quality = 90;

        /// <summary>
        /// Image quality, default is 90
        /// 图片质量
        /// </summary>
        public int Quality
        {
            get { return quality; }
            set
            {
                if (value < 1 || value > 100)
                {
                    throw new ArgumentOutOfRangeException("Quality must be between 1 and 100");
                }
                quality = value;
            }
        }

        private float resizeFactor = 1.25f;

        /// <summary>
        /// Resize factor, 1 to 5, default is 1.25, resize the image to 125% of the display size
        /// 调整大小因子，1 到 5，默认为 1.25，调整图片到显示大小
        /// </summary>
        public float ResizeFactor
        {
            get { return resizeFactor; }
            set
            {
                if (value < 1 || value > 5)
                {
                    throw new ArgumentOutOfRangeException("Resize factor must be between 1 and 5");
                }
                resizeFactor = value;
            }
        }

        private readonly Image image;
        private readonly IImageFormat format;
        private int width;
        private int height;

        private PngCompressionLevel pngCompressionLevel
        {
            get
            {
                var value = 10 - (int)Math.Round(quality / 10.0, 0);
                if (!Enum.TryParse<PngCompressionLevel>(value.ToString(), out var level))
                {
                    level = PngCompressionLevel.DefaultCompression;
                }
                return level;
            }
        }

        public PdfImage(Image image) : base(PdfChunkType.Image)
        {
            this.image = image;

            var format = image.Metadata.DecodedImageFormat;
            if (format == null || !SupportedFormats.Contains(format))
            {
                throw new NotSupportedException("Image format not supported");
            }
            this.format = format;
        }

        public override Task CalculatePositionAsync(IPdfPage page, PdfBlockLine line, PdfBlockLineChunk chunk)
        {
            var point = page.CalculatePoint(chunk.StartPoint);

            // Image rendering starts from bottom to top
            // width & height is different from the text rendering
            var pointBytes = PdfOperator.Tm(width, 0, 0, height, point.X, point.Y - height, true);

            chunk.InsertAfter(pointBytes, PdfOperator.q);
            return Task.CompletedTask;
        }

        public override async Task<bool> WriteInnerAsync(PdfWriter writer, PdfStyle style, System.Drawing.RectangleF rect, PdfPoint point, PdfBlockLine line, Func<PdfBlockLine, PdfBlockLine?, Task> newLineAction)
        {
            // Image size
            var imageWidth = image.Width;
            var imageHeight = image.Height;

            // Target image display size
            (width, height) = style.GetSize(imageWidth, imageHeight, writer.CurrentPage.ContentRect);

            // Resize size
            var resizeWidth = (int)(width * resizeFactor);
            if (resizeWidth > imageWidth)
            {
                resizeWidth = imageWidth;
            }

            var resizeHeight = (int)(height * resizeFactor);
            if (resizeHeight > imageHeight)
            {
                resizeHeight = imageHeight;
            }


            // Auto resize to save space
            if (resizeWidth != image.Width || resizeHeight != image.Height)
            {
                image.Mutate(x => x.Resize(resizeWidth, resizeHeight));
            }

            // Unify the color space to RGB to reduce complexity
            var colorSpace = new PdfName("DeviceRGB");

            IFilter[]? filters = null;
            PdfImageStream? sMaskStream = null;
            ReadOnlyMemory<byte> bytes;
            if (format.Equals(PngFormat.Instance))
            {
                var meta = image.Metadata.GetPngMetadata();
                var bitDepth = meta.BitDepth ?? PngBitDepth.Bit8;

                var encoder = new PngEncoder
                {
                    // https://docs.sixlabors.com/api/ImageSharp/SixLabors.ImageSharp.Formats.Png.PngColorType.html
                    ColorType = PngColorType.RgbWithAlpha,
                    CompressionLevel = pngCompressionLevel,
                    BitDepth = bitDepth,
                    InterlaceMethod = PngInterlaceMode.None,
                    SkipMetadata = true,

                    // https://docs.sixlabors.com/api/ImageSharp/SixLabors.ImageSharp.Formats.Png.PngFilterMethod.html
                    FilterMethod = PngFilterMethod.None
                };

                var png = await PdfImagePng.ParseAsync(await ReadStreamAsync(image, encoder));

                var (imageBytes, maskBytes) = await png.BuildPdfDataAsync();

                bytes = imageBytes;

                sMaskStream = new PdfImageStream(maskBytes, resizeWidth, resizeHeight)
                {
                    BitsPerComponent = 8,
                    ColorSpace = new PdfName("DeviceGray"),
                    Obj = writer.CreateObj()
                };
            }
            else
            {
                // Source quality: image.Metadata.GetJpegMetadata().Quality
                // YCbCrRatio444 is the best quality with less size than RGB (tested)
                var encoder = new JpegEncoder
                {
                    Quality = Quality,
                    ColorType = JpegEncodingColor.YCbCrRatio444,
                    SkipMetadata = true
                };

                bytes = await ReadBytesAsync(image, encoder);

                filters = PdfDocument.Debug ? [new DCTFilter()] : [new FlateFilter(), new DCTFilter()];
            }

            // image.PixelType.BitsPerPixel = 24
            var imageStream = new PdfImageStream(bytes, resizeWidth, resizeHeight)
            {
                BitsPerComponent = 8,
                ColorSpace = colorSpace,
                Filters = filters,
                SMaskStream = sMaskStream
            };

            var imageRef = writer.WriteImage(imageStream);

            // Operators
            var operators = new List<byte[]>
            {
                PdfOperator.q,
                PdfOperator.Do(imageRef)
            };

            // New chunk
            var chunk = new PdfBlockLineChunk(null, height, StartPoint, false)
            {
                Owner = this,
                Operators = operators,
                Style = style
            };
            line.AddChunk(chunk);

            // Restore graphics state
            chunk.EndOperators.AddRange([PdfOperator.Q]);

            return false;
        }
    }
}
