namespace com.etsoo.EasyPdf.Types
{
    /// <summary>
    /// PDF basic dictionary
    /// PDF 基础字典数据
    /// </summary>
    public record PdfDictionaryBase<K, V>(Dictionary<K, V> Value) : IPdfType<Dictionary<K, V>>
        where K : IPdfType
        where V : IPdfType
    {
        /// <summary>
        /// Get item
        /// 获取项目
        /// </summary>
        /// <typeparam name="T">Generic item type</typeparam>
        /// <param name="key">Key name</param>
        /// <returns>Value</returns>
        public T? Get<T>(string key) where T : class, IPdfType
        {
            var item = Value.FirstOrDefault(p => p.Key.KeyEquals(key)).Value;
            if (item == null) return default;
            return item as T;
        }

        /// <summary>
        /// Get required item
        /// 获取所需的项目
        /// </summary>
        /// <typeparam name="T">Generic item type</typeparam>
        /// <param name="key">Key name</param>
        /// <returns>Value</returns>
        public T GetRequired<T>(string key) where T : class, IPdfType
        {
            var v = Get<T>(key);
            return v ?? throw new ArgumentException(key);
        }

        /// <summary>
        /// Get item value
        /// 获取项目值
        /// </summary>
        /// <typeparam name="T">Generic value item type</typeparam>
        /// <param name="key">Key name</param>
        /// <returns>Value</returns>
        public T? GetValue<T>(string key)
        {
            var item = Get<IPdfType<T>>(key);
            if (item == null) return default;
            return item.Value;
        }

        /// <summary>
        /// Get required item value
        /// 获取所需的项目值
        /// </summary>
        /// <typeparam name="T">Generic value item type</typeparam>
        /// <param name="key">Key name</param>
        /// <returns>Value</returns>
        /// <exception cref="ArgumentException"></exception>
        public T GetRequiredValue<T>(string key)
        {
            var v = GetValue<T>(key);
            return v ?? throw new ArgumentException(key);
        }

        /// <summary>
        /// Is equal to key
        /// 是否键相等
        /// </summary>
        /// <param name="item">Key value</param>
        /// <returns>Result</returns>
        public bool KeyEquals(string item)
        {
            return false;
        }

        /// <summary>
        /// Write to stream
        /// 写入流
        /// </summary>
        /// <param name="stream">Stream to write</param>
        /// <returns>Task</returns>
        public virtual async Task WriteToAsync(Stream stream)
        {
            stream.Write(PdfConstants.DictionaryStartBytes);

            var index = 0;
            foreach (var item in Value)
            {
                if (index > 0)
                {
                    stream.WriteByte(PdfConstants.LineFeedByte);
                }

                await item.Key.WriteToAsync(stream);
                stream.WriteByte(PdfConstants.SpaceByte);
                await item.Value.WriteToAsync(stream);

                index++;
            }

            stream.Write(PdfConstants.DictionaryEndBytes);
        }
    }
}
