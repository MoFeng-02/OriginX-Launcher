using System.Collections.Generic;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MFToolkit.Abstractions.DependencyInjection;
using MFToolkit.Avaloniaui.Routes.Core.Abstractions;
using MFToolkit.Minecraft.Entities.Versions;
using MFToolkit.Minecraft.Enums;
using MFToolkit.Minecraft.Extensions;
using OriginX.Abstractions;
using OriginX.Features.Resources.VersionManage.Datas;
using OriginX.ViewModels.Versions.Modules;

namespace OriginX.ViewModels.Versions;

[Scoped]
public partial class VersionDownloadCurrentViewModel : PageViewModel, IQueryAttributable
{
    #region DI

    private readonly DownloadItemsViewModel _downloadItemsViewModel;

    public VersionDownloadCurrentViewModel(DownloadItemsViewModel downloadItemsViewModel)
    {
        _downloadItemsViewModel = downloadItemsViewModel;
    }

    #endregion

    #region 属性

    [ObservableProperty] private VersionInfo? _selectedVersion;

    [ObservableProperty] private ModLoaderType _modLoaderType;

    [ObservableProperty] private List<string> _steps = StepsData.Steps;

    [ObservableProperty] private string? _versionName;

    #endregion

    public void ApplyQueryAttributes(IDictionary<string, object?> parameters)
    {
        if (!parameters.TryGetValue("version", out var version)) return;
        parameters.TryGetValue("modLoader", out var modLoader);
        if (version is not VersionInfo versionInfo) return;
        SelectedVersion = versionInfo;
        if (modLoader is ModLoaderType modLoaderType)
        {
            ModLoaderType = modLoaderType;
        }

        VersionName = SelectedVersion.Id +
                      (ModLoaderType == ModLoaderType.None ? "" : "_" + ModLoaderType.GetDisplayName());
    }

    #region 方法组

    [RelayCommand]
    private async Task DownloadAsync()
    {
        await _downloadItemsViewModel.DownloadAsync(SelectedVersion, VersionName, ModLoaderType);
    }

    #endregion
}