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
        /// <param name="dic">Dictionary data</param>
        public PdfObjectDic(PdfObject? obj = null, PdfDictionary? dic = null)
        {
            Obj = obj;
            Dic = dic ?? new PdfDictionary([]);
        }

        protected virtual void AddItems()
        {
            // Type
            Dic.AddNames(nameof(Type), Type);
        }

        protected virtual async Task WriteOthersAsync(Stream stream)
        {
            await Task.CompletedTask;
        }

        public async Task WriteToAsync(Stream stream)
        {
            // Check obj
            if (Obj == null)
            {
                throw new NullReferenceException(nameof(Obj));
            }

            // 1 0 obj in single line
            await Obj.WriteToAsync(stream);
            stream.WriteByte(PdfConstants.LineFeedByte);

            // Add custom items
            AddItems();

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
