using Avalonia.Controls;
using MFToolkit.Abstractions.DependencyInjection;

namespace OriginX.Features.Settings;

[Transient]
public partial class SettingsPage : UserControl
{
    public SettingsPage()
    {
        InitializeComponent();
    }
}