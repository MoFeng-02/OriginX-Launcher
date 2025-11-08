using Avalonia.Controls;
using Avalonia.Input;
using MFToolkit.Abstractions.DependencyInjection;
using OriginX.ViewModels.Versions;

namespace OriginX.Features.Resources.VersionManage;

[Singleton]
public partial class VersionDownloadSelectPage : UserControl
{
    private VersionDownloadViewModel? _vm;
    public VersionDownloadSelectPage()
    {
        InitializeComponent();
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();
        _vm = DataContext as VersionDownloadViewModel;
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
            _vm?.SearchVersionMethod();
        }
    }
}