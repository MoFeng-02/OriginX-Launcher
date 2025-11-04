using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MFToolkit.Abstractions.DependencyInjection;
using SukiUI;
using SukiUI.Models;

namespace OriginX.ViewModels.CustomTheme;

[Singleton]
public partial class CustomThemeDialogViewModel(SukiTheme theme) : ObservableObject
{
    [ObservableProperty] private string _displayName = "";
    [ObservableProperty] private Color _primaryColor = Colors.White;

    [ObservableProperty] private Color _accentColor = Colors.Silver;

// 主色调画刷（供控件直接绑定）
    public SolidColorBrush PrimaryBrush => new SolidColorBrush(PrimaryColor);

    // 强调色画刷（供控件直接绑定）
    public SolidColorBrush AccentBrush => new SolidColorBrush(AccentColor);

// 当主色调变化时，更新画刷通知
    partial void OnPrimaryColorChanged(Color value)
    {
        OnPropertyChanged(nameof(PrimaryBrush)); // 通知UI PrimaryBrush已更新
    }

    // 当强调色变化时，更新画刷通知
    partial void OnAccentColorChanged(Color value)
    {
        OnPropertyChanged(nameof(AccentBrush)); // 通知UI AccentBrush已更新
    }

    [RelayCommand]
    private void TryCreateTheme()
    {
        if (string.IsNullOrEmpty(DisplayName)) return;
        var theme1 = new SukiColorTheme(DisplayName, PrimaryColor, AccentColor);
        theme.AddColorTheme(theme1);
        theme.ChangeColorTheme(theme1);
    }
}