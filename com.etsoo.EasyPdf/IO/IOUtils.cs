using Microsoft.IO;

namespace com.etsoo.EasyPdf.IO
{
    /// <summary>
    /// IO Utilities
    /// IO 工具
    /// </summary>
    public static class IOUtils
    {
        /// <summary>
        /// Stream manager
        /// 流管理器
        /// </summary>
        public static readonly RecyclableMemoryStreamManager StreamManager = new RecyclableMemoryStreamManager();
    }
}
