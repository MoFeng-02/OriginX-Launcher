using System;
using System.Collections.Generic;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MFToolkit.Abstractions.DependencyInjection;
using MFToolkit.Avaloniaui.Helpers;
using OriginX.Abstractions;

namespace OriginX.ViewModels;

[Singleton]
public partial class PlayGameViewModel : PageViewModel
{
    // 玩家信息
    [ObservableProperty] private string _playerName = "Steve";

    [ObservableProperty] private Bitmap? _playerAvatar;

    // 版本信息
    [ObservableProperty] private List<string> _availableVersions =
    [
        "1.20.1",
        "1.19.4",
        "1.18.2",
        "1.17.1",
        "1.16.5"
    ];

    [ObservableProperty] private string? _selectedVersion = "1.20.1";

    public PlayGameViewModel()
    {
        // 加载默认头像（实际应用中应从用户数据加载）
        LoadDefaultAvatar();
    }

    private void LoadDefaultAvatar()
    {
        // 在实际应用中，这里应该加载用户的头像
        // 这里使用占位逻辑
        PlayerAvatar = ImageHelper.LoadFromResource(new Uri("avares://OriginX/Assets/Logo/OriginX.png"));
    }

    // 命令
    [RelayCommand]
    private void SwitchAccount()
    {
        // 切换账号逻辑
    }

    [RelayCommand]
    private void OpenVersionSettings()
    {
        // 打开版本设置逻辑
    }

    [RelayCommand]
    private void LaunchGame()
    {
        // 启动游戏逻辑
    }
}