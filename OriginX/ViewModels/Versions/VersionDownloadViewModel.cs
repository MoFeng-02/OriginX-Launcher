using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MFToolkit.Abstractions.DependencyInjection;
using MFToolkit.Avaloniaui.Routes.Core.Abstractions;
using MFToolkit.Avaloniaui.Routes.Core.Interfaces;
using MFToolkit.Minecraft.Entities.GameVersion;
using MFToolkit.Minecraft.Entities.Versions;
using MFToolkit.Minecraft.Enums;
using MFToolkit.Minecraft.Extensions;
using MFToolkit.Minecraft.Options;
using MFToolkit.Minecraft.Services.Downloads.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OriginX.Abstractions;
using OriginX.Features.Resources.VersionManage.Datas;

namespace OriginX.ViewModels.Versions;

[Scoped]
public partial class VersionDownloadViewModel : PageViewModel, IQueryAttributable
{
    private readonly IMinecraftVersionService minecraftVersionService;
    private readonly INavigationService navigationService;
    private readonly ILogger<VersionDownloadViewModel> _logger;
    private readonly CurrentDownloadInfo currentDownloadInfo;

    private readonly IOptionsMonitor<StorageOptions> storageOptions;

    public VersionDownloadViewModel(IMinecraftVersionService minecraftVersionService,
        INavigationService navigationService,
        ILogger<VersionDownloadViewModel> logger,
        CurrentDownloadInfo currentDownloadInfo,
        IOptionsMonitor<StorageOptions> storageOptions)
    {
        this.minecraftVersionService = minecraftVersionService;
        this.navigationService = navigationService;
        this._logger = logger;
        this.currentDownloadInfo = currentDownloadInfo;
        this.storageOptions = storageOptions;
    }


    #region 参数

    /// <summary>
    /// 加载
    /// </summary>
    [ObservableProperty] private bool _loading;

    /// <summary>
    /// 搜索版本
    /// </summary>
    [ObservableProperty] private string? _searchVersion;

    /// <summary>
    /// 搜索隐藏版本类型
    /// </summary>
    [ObservableProperty] private bool _searchShowVersionType = true;

    /// <summary>
    /// 版本全
    /// </summary>
    private VersionManifest? _versionManifest;

    /// <summary>
    /// 版本列表
    /// </summary>
    [ObservableProperty] private ObservableCollection<VersionInfo> _versions = [];

    /// <summary>
    /// 版本类型
    /// </summary>
    [ObservableProperty] private VersionType _versionType = VersionType.Release;

    [ObservableProperty] private List<string> _steps = StepsData.Steps;

    public void ApplyQueryAttributes(IDictionary<string, object?> query)
    {
        if (Versions.Count > 0) return;
        Loading = true;
        _logger.LogInformation("进入获取版本信息");
        _ = Task.Run(async () =>
        {
            _logger.LogInformation("在线程中读/获取");
            // 读取是否有版本信息
            try
            {
                var versions = await minecraftVersionService.GetVersionManifestAsync();
                // 读取值
                _versionManifest = versions;
                ObservableCollection<VersionInfo> versionList = [];
                foreach (var item in _versionManifest!.Versions.Where(item => item.VersionType == VersionType))
                {
                    versionList.Add(item);
                }

                Versions = versionList;
                Loading = false;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Loading = false;
            }
        });
    }

    #endregion

    /// <summary>
    /// 刷新
    /// </summary>
    [RelayCommand]
    private async Task RefreshAsync()
    {
        _logger.LogInformation("执行一次刷新");
        Loading = true;
        await minecraftVersionService.GetVersionManifestAsync(isRefresh: true);
        Loading = false;
    }

    /// <summary>
    /// 切换版本值
    /// </summary>
    /// <param name="value"></param>
    [RelayCommand]
    private void ChangeValue(string value)
    {
        var versionType = value.ToVersionType();
        if (!IsVersionChecked(versionType) || versionType == VersionType.None) return;
        VersionType = versionType;
        ChangeVersions();
    }

    /// <summary>
    /// 跳转到指定的版本页面
    /// </summary>
    /// <param name="version"></param>
    [RelayCommand]
    private async Task GoToVersionAsync(GameVersionInfo version)
    {
        _logger.LogInformation("执行一次跳转选择加载器版本页面");
        await navigationService.GoToAsync("download/" + version.Id + "/" + version.Type);
    }

    /// <summary>
    /// 检查版本是否为定义版本
    /// </summary>
    /// <param name="value"></param>
    private static bool IsVersionChecked(VersionType value)
    {
        return value is VersionType.Release or VersionType.Snapshot
            or VersionType.OldAlpha or VersionType.OldBeta;
    }

    /// <summary>
    /// 切换值
    /// </summary>
    /// <returns></returns>
    private void ChangeVersions()
    {
        Loading = true;
        Versions.Clear();
        foreach (var item in _versionManifest!.Versions.Where(item => item.VersionType == VersionType))
        {
            Versions.Add(item);
        }

        Loading = false;
    }


    internal void SearchVersionMethod()
    {
        Loading = true;
        Versions.Clear();
        if (string.IsNullOrEmpty(SearchVersion))
        {
            SearchShowVersionType = true;
            foreach (var item in _versionManifest!.Versions.Where(item => item.VersionType == VersionType))
            {
                Versions.Add(item);
            }

            Loading = false;
            return;
        }

        SearchShowVersionType = false;
        foreach (var item in _versionManifest!.Versions.Where(item => item.Id.Contains(SearchVersion)))
        {
            Versions.Add(item);
        }

        Loading = false;
    }
}

/// <summary>
/// 当前下载选择信息
/// </summary>
[Singleton]
public class CurrentDownloadInfo
{
    #region 原版

    /// <summary>
    /// 当前选择版本信息
    /// </summary>
    public GameVersionInfo? MinecraftVersion { get; set; }

    #endregion

    #region Mod loader 加载器

    /// <summary>
    /// 加载器信息，字典处理
    /// </summary>
    public Dictionary<string, string> LoaderInfo { get; set; } = [];

    /// <summary>
    /// 设置加载器主要信息
    /// </summary>
    /// <param name="loaderName"></param>
    /// <param name="loaderVersion"></param>
    public void SetLoaderInfo(string loaderName, string loaderVersion)
    {
        LoaderInfo[loaderName] = loaderVersion;
    }

    /// <summary>
    /// 清理当前选择的加载器信息
    /// </summary>
    public void ClearLoaderInfo()
    {
        LoaderInfo.Clear();
    }

    #endregion
}