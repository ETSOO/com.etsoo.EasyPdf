namespace com.etsoo.EasyPdf.Types
{
    /// <summary>
    /// PDF type interface
    /// PDF 类型接口
    /// </summary>
    public interface IPdfType
    {
        /// <summary>
        /// Is equal to the string key item
        /// 是否等于字符串键项
        /// </summary>
        /// <param name="item">String item</param>
        /// <returns>Result</returns>
        bool KeyEquals(string item);

        /// <summary>
        /// Write to stream
        /// 写入流
        /// </summary>
        /// <param name="stream">Save stream</param>
        /// <returns>Task</returns>
        Task WriteToAsync(Stream stream);
    }

    /// <summary>
    /// Generic PDF type interface
    /// 泛型 PDF 类型接口
    /// </summary>
    internal interface IPdfType<T> : IPdfType
    {
        /// <summary>
        /// Value
        /// 值
        /// </summary>
        T Value { get; }
    }
}
