using com.etsoo.EasyPdf.Types;
using System.Drawing;

namespace com.etsoo.EasyPdf.Objects
{
    /// <summary>
    /// PDF annotation object
    /// 12.5.2 Annotation Dictionaries
    /// PDF 注释对象
    /// </summary>
    internal class PdfAnnotation : PdfObjectDic
    {
        public override string Type => "Annot";

        /// <summary>
        /// The type of annotation
        /// 注释类型
        /// </summary>
        public string Subtype { get; }

        /// <summary>
        /// The annotation rectangle, defining the location of the annotation on the page in default user space units
        /// 注释矩形，定义注释在页面上的位置，以默认用户空间单位表示
        /// </summary>
        public Rectangle Rect { get; set; }

        /// <summary>
        /// Text that shall be displayed for the annotation or, if this type of annotation does not display text, an alternate description of the annotation’s contents in human-readable form
        /// 文本，应显示为注释的内容，或者，如果此类型的注释不显示文本，则以人类可读形式提供注释内容的替代描述
        /// </summary>
        public string? Contents { get; set; }

        /// <summary>
        /// The annotation’s name, a text string uniquely identifying it among all the annotations on its page
        /// 注释名称，一个文本字符串，用于在其页面上唯一标识它
        /// </summary>
        public string? NM { get; set; }

        /// <summary>
        /// The date and time when the annotation was most recently modified
        /// 日期和时间，注释最近修改的时间
        /// </summary>
        public PdfDateTime? M { get; set; }

        /// <summary>
        /// The annotation’s flags specifying various characteristics of the annotation
        /// 注释的标志，指定注释的各种特性
        /// </summary>
        public int F { get; set; } = 0;

        /// <summary>
        /// The appearance dictionary for the annotation
        /// 显示字典
        /// </summary>
        public PdfDictionary? AP { get; set; }

        /// <summary>
        /// The appearance state of the annotation
        /// 显示状态
        /// </summary>
        public string? AS { get; set; }

        /// <summary>
        /// An array specifying the characteristics of the annotation’s border
        /// 数组，指定注释边框的特性
        /// </summary>
        public PdfArray Border { get; set; } = new PdfArray(0, 0, 1);

        /// <summary>
        /// Border style
        /// 边框样式
        /// </summary>
        public PdfAnnotationBS? BS { get; set; }

        /// <summary>
        /// Representing a color used for the interior of the annotation
        /// 用于注释内部的颜色
        /// </summary>
        public PdfRealArray? C { get; set; }

        /// <summary>
        /// The integer key of the annotation’s entry in the structural parent tree
        /// 整数键，注释在结构父树中的条目
        /// </summary>
        public int? StructParent { get; set; }

        /// <summary>
        /// An optional content group or optional content membership dictionary
        /// 可选内容组或可选内容成员资格字典
        /// </summary>
        public PdfDictionary? OC { get; set; }

        public PdfAnnotation(string subtype, Rectangle rect) : base()
        {
            Subtype = subtype;
            Rect = rect;
        }

        protected override void AddItems()
        {
            base.AddItems();

            Dic.AddNameStr(nameof(Subtype), Subtype);
            Dic.AddNameRect(nameof(Rect), Rect);
            Dic.AddNameStr(nameof(Contents), Contents);
            Dic.AddNameStr(nameof(NM), NM);
            Dic.AddNameItem(nameof(M), M);
            Dic.AddNameInt(nameof(F), F);
            Dic.AddNameItem(nameof(AP), AP);
            Dic.AddNameStr(nameof(AS), AS);
            Dic.AddNameItem(nameof(Border), Border);
            Dic.AddNameItem(nameof(BS), BS);
            Dic.AddNameItem(nameof(C), C);
            Dic.AddNameInt(nameof(StructParent), StructParent);
            Dic.AddNameItem(nameof(OC), OC);
        }
    }

    internal record PdfAnnotationBS : PdfDictionary
    {
        public const string Solid = "S";
        public const string Dashed = "D";
        public const string Beveled = "B";
        public const string Inset = "I";
        public const string Underline = "U";

        public int? W { get; set; }

        public PdfName? S { get; set; }

        public PdfIntArray? D { get; set; }

        public override Task WriteToAsync(Stream stream)
        {
            AddNameInt("W", W);
            AddNameItem("S", S);
            AddNameItem("D", D);

            return base.WriteToAsync(stream);
        }
    }
}
