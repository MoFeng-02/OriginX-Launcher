using Avalonia.Controls;
using MFToolkit.Abstractions.DependencyInjection;

namespace OriginX.Features.Home;

[Singleton]
public partial class PlayGamePage : UserControl
{
    public PlayGamePage()
    {
        InitializeComponent();
    }
}