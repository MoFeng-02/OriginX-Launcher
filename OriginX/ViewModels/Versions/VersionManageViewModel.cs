using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls.Notifications;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MFToolkit.Abstractions.DependencyInjection;
using MFToolkit.Avaloniaui.Routes.Core.Interfaces;
using MFToolkit.Minecraft.Services.Auth;
using MFToolkit.Minecraft.Services.Auth.Interfaces;
using Microsoft.Extensions.Options;
using NPOI.SS.Formula.Functions;
using OriginX.Abstractions;
using OriginX.Features.Resources.VersionManage;
using OriginX.Minecraft.Abstractions;
using OriginX.Models.Versions;
using OriginX.Options.MinecraftOptions;
using OriginX.Utils;
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
    [ObservableProperty] private VersionInfo? _currentVersion;

    /// <summary>
    /// 当前选择分组
    /// </summary>
    [ObservableProperty] private VersionGroup? _currentVersionGroup;

    /// <summary>
    /// 当前分组清单
    /// </summary>
    [ObservableProperty] private ObservableCollection<VersionGroup> _versionGroups = [];

    /// <summary>
    /// 当前分组的所有版本
    /// </summary>
    [ObservableProperty] private ObservableCollection<VersionInfo> _versions = [];

    [ObservableProperty] private bool _isLoading;

    /// <summary>
    /// 版本列表
    /// </summary>
    [ObservableProperty] private bool _isVersionList = true;

    #endregion


    #region 依赖服务

    /// <summary>
    /// 已存在版本的管理器
    /// </summary>
    private readonly IVersionManager _versionManager;

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

    public VersionManageViewModel(IVersionManager versionManager, ISukiToastManager toastManager,
        ISukiDialogManager dialogManager, IOptionsMonitor<StorageOptions> storageOptions, INavigationService navigationService, IOfficialAuthService officialAuthService)
    {
        _versionManager = versionManager;
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
        // 当CurrentVersionGroup（选中分组）变化时
        if (e.PropertyName == nameof(CurrentVersionGroup))
        {
            _ = LoadVersionsByCurrentGroupAsync();
        }

        base.OnPropertyChanged(e);
    }

    #endregion

    /// <summary>
    /// 加载所有版本分组（首次加载/刷新时用）
    /// </summary>
    private async Task LoadVersionGroupsAsync()
    {
        try
        {
            IsLoading = true;

            // 1. 后台获取分组数据
            var groups = await _versionManager.GetVersionsGroupedByStorageAndRootAsync();

            // 2. 切换到UI线程更新分组集合
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                VersionGroups.Clear();
                foreach (var group in groups)
                {
                    VersionGroups.Add(group);
                }

                // 自动选中第一个分组（如果有）
                if (VersionGroups.Any() && CurrentVersionGroup == null)
                {
                    CurrentVersionGroup = VersionGroups.First();
                }
            });
        }
        catch (Exception ex)
        {
            _toastManager.CreateToast().OfType(NotificationType.Error).WithTitle("加载失败")
                .WithContent($"版本分组加载出错：{ex.Message}").Queue();
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// 根据当前选中的分组，加载对应的版本列表
    /// </summary>
    private async Task LoadVersionsByCurrentGroupAsync()
    {
        try
        {
            IsLoading = true;
            Versions.Clear(); // 先清空当前列表

            if (CurrentVersionGroup == null)
                return; // 没有选中分组，直接返回

            // 1. 后台获取该分组下的所有完整版本信息
            // （这里需要根据分组的存储模式+根路径筛选，复用之前的服务方法）
            var groupVersions = await _versionManager.GetVersionsByStorageModeAsync(CurrentVersionGroup.StorageMode);

            // 2. 进一步筛选出该分组下所有子根目录的版本
            var targetVersions = groupVersions
                .Where(v => CurrentVersionGroup.RootGroups.Keys.Contains(
                    VersionBriefInfo.FromVersionInfo(v).RootPath)) // 用精简信息的根路径匹配
                .OrderByDescending(v => v.IsFavorite) // 收藏的排前面
                .ThenBy(v => v.DisplayName)
                .ToList();

            // 3. 切换到UI线程更新版本列表
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                foreach (var version in targetVersions)
                {
                    Versions.Add(version);
                }

                // 自动选中第一个版本（可选）
                if (Versions.Any() && CurrentVersion == null)
                {
                    CurrentVersion = Versions.First<VersionInfo>();
                }
            });
        }
        catch (Exception ex)
        {
            _toastManager.CreateToast().OfType(NotificationType.Error).WithTitle("加载失败")
                .WithContent($"版本列表加载出错：{ex.Message}").Queue();
        }
        finally
        {
            IsLoading = false;
        }
    }

    #region 方法

    /// <summary>
    /// 前往下载新版本
    /// </summary>
    [RelayCommand]
    private async Task ToDownloadVersionAsync()
    {
        var authData = await _officialAuthService.GetMicrosoftDeviceCodeAsync(["XboxLive.signin", "offline_access"]);
        // 复制 user code
        if (ClipboardUtil.Clipboard != null)
            await ClipboardUtil.Clipboard.SetTextAsync(authData.UserCode);
        // 调起系统浏览器
        Process.Start(new ProcessStartInfo(authData.VerificationUri!)
        {
            UseShellExecute = true // 使用系统Shell启动，自动调用默认浏览器
        });
        var token = await _officialAuthService.LoginWithMicrosoftDeviceCodeAsync(authData.DeviceCode!, authData.Interval,
            authData.ExpiresAt);
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