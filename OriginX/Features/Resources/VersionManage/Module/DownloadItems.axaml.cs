using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using MFToolkit.Abstractions.DependencyInjection;
using MFToolkit.App;
using MFToolkit.Minecraft.Enums;
using MFToolkit.Minecraft.Options;
using OriginX.ViewModels.Versions.Modules;

namespace OriginX.Features.Resources.VersionManage.Module;

/// <summary>
/// 用于提供和展示的一个特殊模组,需要使用特殊的来给他获取了
/// </summary>
[Singleton]
public partial class DownloadItems : UserControl
{
    private readonly DownloadItemsViewModel _viewModel;

    public DownloadItems()
    {
        InitializeComponent();
        DataContext = _viewModel = MFApp.GetService<DownloadItemsViewModel>() ??
                                   throw new Exception("未注入" + nameof(DownloadItemsViewModel));
    }

    // public async Task DownloadAsync(string versionId,
    //     string? versionName = null,
    //     ModLoaderType modLoaderType = ModLoaderType.None,
    //     StorageOptions? storageOptions = null)
    // {
    //     await _viewModel.DownloadAsync(versionId, versionName, modLoaderType, storageOptions);
    // }
}