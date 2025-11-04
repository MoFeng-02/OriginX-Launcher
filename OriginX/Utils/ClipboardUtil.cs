using Avalonia;
using Avalonia.Controls;
using Avalonia.Input.Platform;

namespace OriginX.Utils;

/// <summary>
/// 剪贴板
/// </summary>
public static class ClipboardUtil
{
    private static IClipboard? _cipboard { get; set; }
    private static Window? Window { get; set; }

    public static IClipboard? Clipboard => Window != null ? Window.Clipboard : _cipboard;


    /// <summary>
    /// 设置TopLevel
    /// <para>参考：https://docs.avaloniaui.net/zh-Hans/docs/concepts/services/clipboard#%E5%88%9B%E5%BB%BA%E8%A6%81%E5%8F%91%E9%80%81%E5%88%B0%E5%89%AA%E8%B4%B4%E6%9D%BF%E7%9A%84-dataobject</para>
    /// </summary>
    /// <param name="visual"></param>
    public static void SetClipboard(Visual visual)
    {
        _cipboard = TopLevel.GetTopLevel(visual)?.Clipboard;
    }

    /// <summary>
    /// 设置Window
    /// </summary>
    /// <param name="window"></param>
    public static void SetWindow(Window window)
    {
        Window = window;
    }
}