using Avalonia.Controls;
using Avalonia.Input;
using MFToolkit.Abstractions.DependencyInjection;
using OriginX.ViewModels.Versions;

namespace OriginX.Features.Resources.VersionManage;

[Singleton]
public partial class VersionDownloadSelectPage : UserControl
{
    private readonly VersionDownloadSelectViewModel _vm;
    public VersionDownloadSelectPage(VersionDownloadSelectViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
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