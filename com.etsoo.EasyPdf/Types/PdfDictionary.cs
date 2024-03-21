using System.Drawing;

namespace com.etsoo.EasyPdf.Types
{
    /// <summary>
    /// PDF dictionary
    /// PDF 字典数据
    /// </summary>
    public record PdfDictionary : PdfDictionaryBase<IPdfType, IPdfType>
    {
        /// <summary>
        /// Parse bytes to dictionary, for testing and understanding only
        /// 解析字节为字典，仅用于测试和理解
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static PdfDictionary Parse(ReadOnlySpan<byte> bytes)
        {
            // <<
            var items = bytes.Parse();
            var dic = new Dictionary<IPdfType, IPdfType>();
            for (var i = 0; i < items.Length; i+=2)
            {
                dic.Add(items[i], items[i+1]);
            }
            return new PdfDictionary(dic);
        }

        public PdfDictionary() : this([])
        {
        }

        public PdfDictionary(Dictionary<IPdfType, IPdfType> Value) : base(Value)
        {
        }

        public void AddNameItem(string name, IPdfType? value)
        {
            if (value == null) return;
            Value[new PdfName(name)] = value;
        }

        public void AddNameBinary(string name, string? value)
        {
            if (!string.IsNullOrEmpty(value)) AddNameItem(name, new PdfBinaryString(value));
        }

        public void AddNameStr(string name, string? value)
        {
            if (!string.IsNullOrEmpty(value)) AddNameItem(name, new PdfString(value));
        }

        public void AddNameDate(string name, DateTime? date)
        {
            if (date.HasValue) AddNameItem(name, new PdfDateTime(date.Value));
        }

        public void AddNameBool(string name, bool? value)
        {
            if (!value.HasValue) return;
            AddNameItem(name, new PdfBoolean(value.Value));
        }

        public void AddNameInt(string name, int? value)
        {
            if (!value.HasValue) return;
            AddNameItem(name, new PdfInt(value.Value));
        }

        public void AddNameNum(string name, float? value)
        {
            if (!value.HasValue) return;
            AddNameItem(name, new PdfReal(value.Value));
        }

        public void AddNameNum(string name, double? value)
        {
            if (!value.HasValue) return;
            AddNameItem(name, new PdfReal(value.Value));
        }

        public void AddNameArray(string name, IEnumerable<IPdfType>? items)
        {
            if (items == null) return;
            AddNameItem(name, new PdfArray(items.ToArray()));
        }

        public void AddNameDic<T>(string name, Dictionary<string, T>? dic) where T : IPdfType
        {
            if (dic == null) return;

            var data = new Dictionary<IPdfType, IPdfType>();
            foreach (var item in dic)
            {
                data[new PdfName(item.Key)] = item.Value;
            }

            AddNameItem(name, new PdfDictionary(data));
        }

        public void AddNameRect(string name, Rectangle? rect)
        {
            if (rect == null) return;
            AddNameItem(name, new PdfRectangle(rect.Value));
        }

        public void AddNameRect(string name, Size? size)
        {
            if (size == null) return;
            AddNameItem(name, new PdfRectangle(new Rectangle(Point.Empty, size.Value)));
        }

        public void AddNameRect(string name, RectangleF? rect)
        {
            if (rect == null) return;
            AddNameItem(name, new PdfRectangleF(rect.Value));
        }

        public void AddNames(string name, string? value)
        {
            if (!string.IsNullOrEmpty(value)) AddNameItem(name, new PdfName(value));
        }
    }
}
