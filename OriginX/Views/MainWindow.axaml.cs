using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using MFToolkit.Abstractions.DependencyInjection;
using MFToolkit.Avaloniaui.Routes.Core.Interfaces;
using OriginX.Features.Settings;
using OriginX.Options.RouteOptions;
using OriginX.ViewModels;
using SukiUI.Controls;
using SukiUI.Models;

namespace OriginX.Views;

[Singleton]
public partial class MainWindow : SukiWindow
{
    private INavigationService _navigationService;
    private PageCurrentViewModel vm;
    public MainWindow(INavigationService navigationService, PageCurrentViewModel vm)
    {
        _navigationService = navigationService;
        this.vm = vm;
        InitializeComponent();
    }

    private void SelectRoute_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        var sukiSideMenu = sender as SukiSideMenu;
        if (sukiSideMenu is null) return;
        var routeInfo = sukiSideMenu.SelectedItem as RouteInfo;
        _navigationService.GoToAsync(routeInfo!.Route);
    }

    /// <summary>
    /// 颜色处理
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    /// <exception cref="NotImplementedException"></exception>
    private void ThemeMenuItem_OnClick(object? sender, RoutedEventArgs e)
    { 
        if (e.Source is not MenuItem mItem) return;
        if (mItem.DataContext is not SukiColorTheme cTheme) return;
        vm.ChangeTheme(cTheme);
    }

    private void GoToSettings_OnClick(object? sender, RoutedEventArgs e)
    {
        _navigationService.GoToAsync(nameof(SettingsPage));
    }
}