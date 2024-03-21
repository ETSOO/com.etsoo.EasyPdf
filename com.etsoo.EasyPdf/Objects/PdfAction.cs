namespace com.etsoo.EasyPdf.Objects
{
    /// <summary>
    /// PDF action
    /// 12.6 Actions
    /// PDF 动作
    /// </summary>
    internal class PdfAction : PdfObjectDic
    {
        public override string Type => "Action";

        /// <summary>
        /// The type of action
        /// 动作类型
        /// </summary>
        public string S { get; init; }

        /// <summary>
        /// The next action or sequence of actions that shall be performed after the action represented by this dictionary
        /// 下一个动作或一系列动作，应在此字典表示的动作之后执行
        /// </summary>
        public IEnumerable<PdfAction>? Next { get; set; }

        public PdfAction(string s)
        {
            S = s;
        }
    }
}
