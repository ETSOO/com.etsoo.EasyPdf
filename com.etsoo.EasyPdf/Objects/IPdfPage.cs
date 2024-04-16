using com.etsoo.EasyPdf.Content;
using com.etsoo.EasyPdf.Support;
using com.etsoo.EasyPdf.Types;
using System.Drawing;
using System.Numerics;

namespace com.etsoo.EasyPdf.Objects
{
    /// <summary>
    /// PDF page object interface
    /// PDF 页面对象接口
    /// </summary>
    public interface IPdfPage
    {
        /// <summary>
        /// Annotation dictionary array
        /// 注释字典数组
        /// </summary>
        List<PdfObject> Annots { get; }

        /// <summary>
        /// Current drawing point inside the content rectangle
        /// 内容矩形内的当前绘制点
        /// </summary>
        PdfPoint CurrentPoint { get; }

        /// <summary>
        /// Page's content rectangle
        /// 页面的内容矩形
        /// </summary>
        RectangleF ContentRect { get; }

        /// <summary>
        /// Page data
        /// 页面数据
        /// </summary>
        PdfPageData Data { get; }

        /// <summary>
        /// Page stream
        /// 页面流
        /// </summary>
        Stream Stream { get; }

        /// <summary>
        /// Style
        /// 样式
        /// </summary>
        PdfStyle Style { get; }

        /// <summary>
        /// Begin text output
        /// 开始文本输出
        /// </summary>
        /// <returns>Task</returns>
        Task BeginTextAsync();

        /// <summary>
        /// Calculate current point
        /// 计算当前点
        /// </summary>
        /// <returns>Result</returns>
        Vector2 CalculatePoint();

        /// <summary>
        /// Calculate point
        /// 计算点
        /// </summary>
        /// <param name="point">Point</param>
        /// <returns>Result</returns>
        Vector2 CalculatePoint(Vector2 point);

        /// <summary>
        /// End text output
        /// 结束文本输出
        /// </summary>
        /// <returns>Task</returns>
        Task EndTextAsync();

        /// <summary>
        /// Move to the current point operator
        /// 移动到当前点操作
        /// </summary>
        /// <param name="adjust">The adjustment</param>
        /// <returns>Bytes</returns>
        byte[] CurrentPointOperator(Vector2? adjust = null);

        /// <summary>
        /// Move to the point
        /// 移动到点
        /// </summary>
        /// <param name="point">The local point</param>
        /// <returns>Global drawing point</returns>
        Task<Vector2> MoveToAsync(Vector2 point);

        /// <summary>
        /// Move text output to the point (cm)
        /// 移动文本输出到点
        /// </summary>
        /// <param name="point">Start point</param>
        /// <param name="lineHeight">Line height</param>
        /// <returns>Global drawing point</returns>
        Task<Vector2> MoveToAsync(Vector2 point, float lineHeight);

        /// <summary>
        /// Restore graphics state
        /// 恢复图形状态
        /// </summary>
        /// <returns>Task</returns>
        Task RestoreStateAsync();

        /// <summary>
        /// Save graphics state
        /// 保持图形状态
        /// </summary>
        /// <returns></returns>
        Task SaveStateAsync();

        /// <summary>
        /// Set font color
        /// 设置字体颜色
        /// </summary>
        /// <param name="color">Color</param>
        /// <returns>Task</returns>
        Task SetFontColor(PdfColor? color);

        /// <summary>
        /// Update point, return global drawing point
        /// 更新点，返回全局绘制点
        /// </summary>
        /// <param name="point">Relative point</param>
        void UpdatePoint(Vector2 point);

        /// <summary>
        /// Write border and background
        /// 输出边框和背景
        /// </summary>
        /// <param name="style">Current style</param>
        /// <param name="rect">Rectangle</param>
        /// <returns>Task</returns>
        ValueTask WriteBorderAndBackgroundAsync(PdfStyle style, RectangleF rect);
    }
}
