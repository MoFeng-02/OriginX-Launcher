using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using MFToolkit.Abstractions.DependencyInjection;
using MFToolkit.Minecraft.Entities.Downloads;
using MFToolkit.Minecraft.Entities.Versions;
using MFToolkit.Minecraft.Enums;
using MFToolkit.Minecraft.Options;
using MFToolkit.Minecraft.Services.Downloads.Interfaces;
using OriginX.Abstractions;
using System.Collections.Specialized;
using System.Linq;
using MFToolkit.Minecraft.Entities.GameVersion;
using MFToolkit.Minecraft.Extensions.VersionExtensions;
using OriginX.Services.VersionServices.Interfaces;

namespace OriginX.ViewModels.Versions.Modules;

[Singleton]
public partial class DownloadItemsViewModel : PageViewModel
{
    private readonly IMinecraftDownloadService _downloadService;
    private readonly IVersionManage _versionManage;
    private bool _isBatchUpdating = false;
    private bool isSetEvent = false;

    public DownloadItemsViewModel(IMinecraftDownloadService downloadService, IVersionManage versionManage)
    {
        _downloadService = downloadService;
        _versionManage = versionManage;

        // 使用包装的集合来减少通知频率
        DownloadProgress = [];

        // 监听原始集合的变化，但进行批量处理
        if (!isSetEvent)
        {
            _downloadService.DownloadQueue.CollectionChanged += OnDownloadQueueChanged;
        }

        isSetEvent = true;
        // 初始数据
        DownloadProgress.AddRange(_downloadService.DownloadQueue);
    }

    [ObservableProperty] private string _versionString;

    [ObservableProperty] private string _versionName;

    [ObservableProperty] private OptimizedObservableCollection<MinecraftDownloadProgress> _downloadProgress;

    private void OnDownloadQueueChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        // 如果是重置操作，批量处理
        if (e.Action == NotifyCollectionChangedAction.Reset)
        {
            DownloadProgress.Clear();
            DownloadProgress.AddRange(_downloadService.DownloadQueue);
            return;
        }

        // 批量添加新项
        if (e.NewItems != null && e.NewItems.Count > 10)
        {
            DownloadProgress.AddRange(e.NewItems.Cast<MinecraftDownloadProgress>());
        }
        else
        {
            // 少量项直接处理
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    if (e.NewItems != null)
                    {
                        foreach (MinecraftDownloadProgress item in e.NewItems)
                        {
                            DownloadProgress.Add(item);
                        }
                    }

                    break;
                case NotifyCollectionChangedAction.Remove:
                    if (e.OldItems != null)
                    {
                        foreach (MinecraftDownloadProgress item in e.OldItems)
                        {
                            DownloadProgress.Remove(item);
                        }
                    }

                    break;
            }
        }
    }

    public async Task DownloadAsync(VersionInfo versionInfo,
        string? versionName = null,
        ModLoaderType modLoaderType = ModLoaderType.None,
        StorageOptions? storageOptions = null)
    {
        _downloadService.ModLoaderType = modLoaderType;
        _downloadService.CompletedInfoAction = async (detail) =>
        {
            await _versionManage.SaveVersionDetailAsync(detail);
            // gameVersionDetail.GetGameArguments()
        };
        await _downloadService.StartDownloadAsync(versionInfo, versionName, storageOptions);
    }


    protected void OnDeactivated()
    {
        _downloadService.DownloadQueue.CollectionChanged -= OnDownloadQueueChanged;
    }
}

/// <summary>
/// 优化的可观察集合，支持批量操作
/// </summary>
public class OptimizedObservableCollection<T> : ObservableCollection<T>
{
    private bool _suppressNotification;

    public void AddRange(IEnumerable<T> items)
    {
        if (items == null) return;

        _suppressNotification = true;
        try
        {
            foreach (var item in items)
            {
                Add(item);
            }
        }
        finally
        {
            _suppressNotification = false;
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
    }

    protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
    {
        if (!_suppressNotification)
        {
            base.OnCollectionChanged(e);
        }
    }
}