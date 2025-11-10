using System.Collections.Generic;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MFToolkit.Abstractions.DependencyInjection;
using MFToolkit.Avaloniaui.Routes.Core.Abstractions;
using MFToolkit.Avaloniaui.Routes.Core.Interfaces;
using MFToolkit.Minecraft.Entities.Versions;
using MFToolkit.Minecraft.Enums;
using MFToolkit.Minecraft.Options;
using OriginX.Abstractions;
using OriginX.Features.Resources.VersionManage;
using OriginX.Features.Resources.VersionManage.Datas;

namespace OriginX.ViewModels.Versions;

[Transient]
public partial class VersionDownloadSelectModLoaderViewModel : PageViewModel, IQueryAttributable
{
    #region DI

    private readonly INavigationService _navigationService;

    public VersionDownloadSelectModLoaderViewModel(INavigationService navigationService)
    {
        _navigationService = navigationService;
    }

    #endregion

    #region 属性

    [ObservableProperty] private List<string> _steps = StepsData.Steps;

    [ObservableProperty] private VersionInfo _selectedVersion = null!;

    [ObservableProperty] private VersionType _versionType;
    [ObservableProperty] private ModLoaderType _modLoaderType;

    [ObservableProperty] private string? _versionString;

    #endregion

    public void ApplyQueryAttributes(IDictionary<string, object?> parameters)
    {
        if (!parameters.TryGetValue("version", out var version)) return;
        if (version is VersionInfo versionInfo)
        {
            VersionType = versionInfo.VersionType;
            VersionString = versionInfo.Id;
            // 获取传递过来的版本块
            SelectedVersion = versionInfo;
        }
    }

    #region 方法组

    [RelayCommand]
    private async Task GoToDownloadAsync()
    {
        await _navigationService.GoToAsync<VersionDownloadCurrentPage>(new()
        {
            { "version", SelectedVersion },
            { "modLoader", ModLoaderType }
        });
    }

    #endregion
}