using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using OriginX.ViewModels.Versions;

namespace OriginX.Features.Resources.VersionManage;

public partial class VersionDownloadSelectPage : UserControl
{
    private readonly VersionDownloadViewModel _vm;
    public VersionDownloadSelectPage(VersionDownloadViewModel vm)
    {
        _vm = vm;
        InitializeComponent();
    }
    
    /// <summary>
    /// 回车用法
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void InputEnter_OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            _vm.SearchVersionMethod();
        }
    }
}