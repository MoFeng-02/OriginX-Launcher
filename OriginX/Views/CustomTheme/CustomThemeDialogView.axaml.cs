using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using MFToolkit.Abstractions.DependencyInjection;

namespace OriginX.Views.CustomTheme;

[Singleton]
public partial class CustomThemeDialogView : UserControl
{
    public CustomThemeDialogView()
    {
        InitializeComponent();
    }
}