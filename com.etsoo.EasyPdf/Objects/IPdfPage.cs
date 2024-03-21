using com.etsoo.EasyPdf.Content;
using com.etsoo.EasyPdf.Dto;
using com.etsoo.EasyPdf.Fonts;
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
        /// End text output
        /// 结束文本输出
        /// </summary>
        /// <returns>Task</returns>
        Task EndTextAsync();

        /// <summary>
        /// Move to the point
        /// 移动到点
        /// </summary>
        /// <param name="point">The point</param>
        /// <returns>Task</returns>
        Task MoveToAsync(Vector2 point);

        /// <summary>
        /// Move text output to the point
        /// 移动文本输出到点
        /// </summary>
        /// <param name="point">Start point</param>
        /// <param name="font">Font</param>
        /// <returns>Task</returns>
        Task MoveToAsync(Vector2 point, IPdfFont font);

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
        /// Write border and background
        /// 输出边框和背景
        /// </summary>
        /// <param name="style">Current style</param>
        /// <param name="rect">Rectangle</param>
        /// <returns>Task</returns>
        ValueTask WriteBorderAndBackgroundAsync(PdfStyle style, RectangleF rect);
    }
}
