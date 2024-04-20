using com.etsoo.EasyPdf.Types;

namespace com.etsoo.EasyPdf.Objects
{
    public class PdfObjectDic
    {
        /// <summary>
        /// Type
        /// 类型
        /// </summary>
        public virtual string Type { get; } = string.Empty;

        /// <summary>
        /// Object
        /// 对象
        /// </summary>
        public virtual PdfObject? Obj { get; set; }

        /// <summary>
        /// Dictionary data
        /// 字典数据
        /// </summary>
        protected PdfDictionary Dic { get; }

        /// <summary>
        /// Constructor
        /// 构造函数
        /// </summary>
        /// <param name="obj">Object</param>
        public PdfObjectDic(PdfObject? obj = null)
        {
            Obj = obj;
            Dic = new PdfDictionary();
        }

        /// <summary>
        /// Add property items
        /// 添加属性项
        /// </summary>
        /// <returns>Task</returns>
        protected virtual Task AddItemsAsync()
        {
            // Type
            Dic.AddNames(nameof(Type), Type);

            return Task.CompletedTask;
        }

        /// <summary>
        /// Write related streams
        /// 写入相关流
        /// </summary>
        /// <param name="writer">Writer</param>
        /// <param name="stream">Stream</param>
        /// <returns>Task</returns>
        public virtual Task WriteRelatedStreams(PdfWriter writer, Stream stream)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Write to stream with other actions
        /// 写入流并执行其他操作
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <returns>Task</returns>
        protected virtual async Task WriteOthersAsync(Stream stream)
        {
            await Task.CompletedTask;
        }

        /// <summary>
        /// Write to stream
        /// 写入流
        /// </summary>
        /// <param name="stream">Stream to write</param>
        /// <returns>Task</returns>
        /// <exception cref="NullReferenceException"></exception>
        public virtual async Task WriteToAsync(Stream stream)
        {
            // Check obj
            if (Obj == null)
            {
                throw new NullReferenceException(nameof(Obj));
            }

            // Line break for clear source code visibility
            stream.WriteByte(13);

            // 1 0 obj in single line
            await Obj.WriteToAsync(stream);
            stream.WriteByte(PdfConstants.LineFeedByte);

            // Add custom items
            await AddItemsAsync();

            // << ... >> dictionary
            await Dic.WriteToAsync(stream);

            // Other (stream) data
            await WriteOthersAsync(stream);

            // endobj in single line
            stream.WriteByte(PdfConstants.LineFeedByte);
            await Obj.WriteEndToAsync(stream);
            stream.WriteByte(PdfConstants.LineFeedByte);
        }
    }
}
