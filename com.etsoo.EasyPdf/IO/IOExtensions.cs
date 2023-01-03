using CommunityToolkit.HighPerformance.Buffers;
using System;
using System.IO;

namespace com.etsoo.EasyPdf.IO
{
    /// <summary>
    /// IO extensions
    /// IO 扩展
    /// </summary>
    public static class IOExtensions
    {
        /// <summary>
        /// Stream to bytes
        /// 转化流为字节
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="bufferSize">Buffer size</param>
        /// <returns>Result</returns>
        public static ReadOnlyMemory<byte> ToBytes(this Stream stream, int bufferSize = 10240)
        {
            var writer = new ArrayPoolBufferWriter<byte>(bufferSize);
            int bytesRead;
            while ((bytesRead = stream.Read(writer.GetSpan(bufferSize))) > 0)
            {
                writer.Advance(bytesRead);
            }

            return writer.WrittenMemory;
        }
    }
}
