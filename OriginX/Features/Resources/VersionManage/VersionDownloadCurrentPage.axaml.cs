using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using MFToolkit.Abstractions.DependencyInjection;

namespace OriginX.Features.Resources.VersionManage;

[Scoped]
public partial class VersionDownloadCurrentPage : UserControl
{
    public VersionDownloadCurrentPage()
    {
        InitializeComponent();
    }
}