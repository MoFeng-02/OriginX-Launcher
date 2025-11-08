using System.Linq;
using System.Threading.Tasks;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MFToolkit.Abstractions.DependencyInjection;
using MFToolkit.Avaloniaui.BaseExtensions;
using MFToolkit.Avaloniaui.Routes.Core.Interfaces;
using OriginX.Options.RouteOptions;
using OriginX.ViewModels.CustomTheme;
using OriginX.Views.CustomTheme;
using SukiUI;
using SukiUI.Dialogs;
using SukiUI.Models;
using SukiUI.Toasts;

namespace OriginX.ViewModels;

[Singleton]
public partial class PageCurrentViewModel : ViewModelBase
{
    /// <summary>
    /// 导航参数
    /// </summary>
    public IAvaloniaReadOnlyList<RouteInfo> Navigations { get; }

    /// <summary>
    /// 土司弹窗
    /// </summary>
    public ISukiToastManager ToastManager { get; }

    /// <summary>
    /// Dialog弹窗
    /// </summary>
    public ISukiDialogManager DialogManager { get; }


    /// <summary>
    /// 主题色集合
    /// </summary>
    public IAvaloniaReadOnlyList<SukiColorTheme> Themes { get; }

    public INavigationService Navigation { get; }

    /// <summary>
    /// 主题
    /// </summary>
    private readonly SukiTheme _theme;

    #region 参数

    /// <summary>
    /// 当前页面
    /// </summary>
    [ObservableProperty] private Control? _currentPage;

    /// <summary>
    /// 当前路由信息
    /// </summary>
    [ObservableProperty] private RouteInfo? _currentRoute;

    /// <summary>
    /// 导航顶级选择
    /// </summary>
    [ObservableProperty] private Control? _selectedPage;

    /// <summary>
    /// 是否显示背景图片
    /// </summary>
    [ObservableProperty] private bool _isShowBackground;

    /// <summary>
    /// 背景图片
    /// </summary>
    [ObservableProperty] private Bitmap? _backgroundImage;

    /// <summary>
    /// 是否有上一级？
    /// </summary>
    [ObservableProperty] private bool _isShowBackNavigation;

    #endregion

    public PageCurrentViewModel(ISukiToastManager toastManager, ISukiDialogManager dialogManager,
        INavigationService navigation)
    {
        ToastManager = toastManager;
        DialogManager = dialogManager;
        Navigation = navigation;
        Navigations =
            new AvaloniaList<RouteInfo>(
                RouteInfoHelper.RouteInfos.Where(q => q.IsTopNavigation));

        _theme = SukiTheme.GetInstance();
        Themes = _theme.ColorThemes;
        Navigation.NavigationCompleted = (page, info) =>
        {
            if (page == null)
            {
                ToastManager.CreateToast()
                    .OfType(NotificationType.Error)
                    .WithTitle("路由不存在")
                    .WithContent("当前页面或路由不存在")
                    .Queue();
                return; // 后面应该要改成错误地址并提供对应错误，或弹窗？
            }

            CurrentPage = (Control)page;
            CurrentPage.DataContext = info!.ViewModel;
            IsShowBackNavigation = Navigation.CanGoBack();
            CurrentRoute = Navigations.FirstOrDefault(q => q.PageType == info?.PageType)!;
        };
    }


    public void ChangeTheme(SukiColorTheme theme) =>
        _theme.ChangeColorTheme(theme);

    #region 方法

    [RelayCommand]
    private void CreateCustomTheme()
    {
        var content = new CustomThemeDialogView()
            { DataContext = new CustomThemeDialogViewModel(_theme) };
        DialogManager.CreateDialog()
            .WithContent(content)
            .Dismiss()
            .ByClickingBackground()
            .TryShow();
    }

    /// <summary>
    /// 返回上一层
    /// </summary>
    [RelayCommand]
    private async Task GoBackAsync()
    {
        await Navigation.GoBackAsync();
    }

    #endregion
}