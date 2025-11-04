using Avalonia.Controls;
using MFToolkit.Abstractions.DependencyInjection;

namespace OriginX.Features.User;

[Singleton]
public partial class UserPage : UserControl
{
    public UserPage()
    {
        InitializeComponent();
    }
}