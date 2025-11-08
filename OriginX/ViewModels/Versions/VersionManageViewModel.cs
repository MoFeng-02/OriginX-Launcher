using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls.Notifications;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MFToolkit.Abstractions.DependencyInjection;
using MFToolkit.Avaloniaui.Routes.Core.Interfaces;
using MFToolkit.Minecraft.Entities.GameVersion;
using MFToolkit.Minecraft.Options;
using MFToolkit.Minecraft.Services.Auth.Interfaces;
using Microsoft.Extensions.Options;
using OriginX.Abstractions;
using OriginX.Features.Resources.VersionManage;
using SukiUI.Dialogs;
using SukiUI.Toasts;

namespace OriginX.ViewModels.Versions;

[Singleton]
public partial class VersionManageViewModel : PageViewModel
{
    #region 属性

    /// <summary>
    /// 当前选择版本
    /// </summary>
    [ObservableProperty] private GameVersionInfo? _currentVersion;

    

    [ObservableProperty] private bool _isLoading;

    /// <summary>
    /// 版本列表
    /// </summary>
    [ObservableProperty] private bool _isVersionList = true;

    #endregion


    #region 依赖服务

    /// <summary>
    /// 当前版本模式（全局）
    /// </summary>
    private readonly IOptionsMonitor<StorageOptions> _storageOptions;

    /// <summary>
    /// 土司弹窗
    /// </summary>
    private readonly ISukiToastManager _toastManager;

    private readonly ISukiDialogManager _dialogManager;

    private readonly INavigationService NavigationService;

    private readonly IOfficialAuthService _officialAuthService;

    public VersionManageViewModel(ISukiToastManager toastManager,
        ISukiDialogManager dialogManager, IOptionsMonitor<StorageOptions> storageOptions, INavigationService navigationService, IOfficialAuthService officialAuthService)
    {
        _toastManager = toastManager;
        _dialogManager = dialogManager;
        _storageOptions = storageOptions;
        NavigationService = navigationService;
        // 加载数据（确保最终在UI线程更新集合）
        _ = LoadVersionGroupsAsync();
        _officialAuthService = officialAuthService;
    }

    #endregion

    #region 监听变化

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
       

        base.OnPropertyChanged(e);
    }

    #endregion

    /// <summary>
    /// 加载所有版本分组（首次加载/刷新时用）
    /// </summary>
    private async Task LoadVersionGroupsAsync()
    {
        
    }

    /// <summary>
    /// 根据当前选中的分组，加载对应的版本列表
    /// </summary>
    private async Task LoadVersionsByCurrentGroupAsync()
    {
        
    }

    #region 方法

    /// <summary>
    /// 前往下载新版本
    /// </summary>
    [RelayCommand]
    private async Task ToDownloadVersionAsync()
    {
        //var authData = await _officialAuthService.GetMicrosoftDeviceCodeAsync(["XboxLive.signin", "offline_access"]);
        //// 复制 user code
        //if (ClipboardUtil.Clipboard != null)
        //    await ClipboardUtil.Clipboard.SetTextAsync(authData.UserCode);
        //// 调起系统浏览器
        //Process.Start(new ProcessStartInfo(authData.VerificationUri!)
        //{
        //    UseShellExecute = true // 使用系统Shell启动，自动调用默认浏览器
        //});
        //var token = await _officialAuthService.LoginWithMicrosoftDeviceCodeAsync(authData.DeviceCode!, authData.Interval,
        //    authData.ExpiresAt);
        await NavigationService.GoToAsync<VersionDownloadSelectPage>();
    }

    /// <summary>
    /// 刷新命令（手动触发重新加载）
    /// </summary>
    [RelayCommand]
    private async Task RefreshAsync()
    {
        // 先重新加载分组，分组变化后会自动触发版本列表更新
        await LoadVersionGroupsAsync();
    }

    #endregion
}