using System;
using System.Collections.Generic;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using Material.Icons;
using MFToolkit.Avaloniaui.Routes.Core.Entities;
using OriginX.Features.Home;
using OriginX.Features.Resources.VersionManage;
using OriginX.Features.Settings;
using OriginX.Features.User;
using OriginX.Language;
using OriginX.ViewModels;
using OriginX.ViewModels.Versions;
using OriginX.Views;
using VersionManageViewModel = OriginX.ViewModels.Versions.VersionManageViewModel;

namespace OriginX.Options.RouteOptions;

/// <summary>
/// 路由信息基类
/// 存储页面路由的核心信息，包括页面类型、路由地址、显示名称等
/// </summary>
public class RouteInfo : RoutingModel
{
    /// <summary>
    /// 显示名称（用于UI展示，如导航菜单文本）
    /// </summary>
    public string? DisplayName { get; }

    /// <summary>
    /// 图标（用于UI展示，如导航菜单图标）
    /// </summary>
    public MaterialIconKind? Icon { get; }


    /// <summary>
    /// 路由信息构造函数
    /// </summary>
    /// <param name="pageType">页面类型（不能为空）</param>
    /// <param name="viewModelType">视图模型类型</param>
    /// <param name="route">路由地址（不能为空或空字符串）</param>
    /// <param name="displayName">显示名称（可选）</param>
    /// <param name="icon">图标（可选）</param>
    /// <param name="isKeepLive">是否保活（默认：false）</param>
    /// <param name="isTopNavigation">是否顶级路由（默认：false）</param>
    /// <param name="priority">优先级（默认：0）</param>
    public RouteInfo(
        Type pageType,
        Type? viewModelType,
        string route,
        string? displayName = null,
        MaterialIconKind? icon = null,
        bool isKeepLive = false,
        bool isTopNavigation = false,
        int priority = 0) : base(pageType, viewModelType, route, isKeepLive, isTopNavigation, priority)
    {
        // 赋值其他属性
        DisplayName = displayName;
        Icon = icon;
    }
}

/// <summary>
/// 泛型路由信息类
/// 简化指定页面类型的路由创建，自动将页面类型绑定为TPage
/// </summary>
/// <typeparam name="TView">页面类型（必须是具体类，不能是接口或抽象类）</typeparam>
/// <typeparam name="TViewModel">页面VM类型</typeparam>
public class RouteInfo<TView, TViewModel> : RouteInfo where TView : Control where TViewModel : ObservableObject
{
    /// <summary>
    /// 泛型路由构造函数
    /// 自动将页面类型设置为typeof(TPage)，简化路由创建
    /// </summary>
    /// <param name="route">路由地址（不能为空或空字符串）</param>
    /// <param name="displayName">显示名称（可选）</param>
    /// <param name="icon">图标（可选）</param>
    /// <param name="isKeepLive">是否保活（默认：false）</param>
    /// <param name="isTopNavigation">是否顶级路由（默认：false）</param>
    /// <param name="priority">优先级（默认：0）</param>
    public RouteInfo(
            string? route = null,
            string? displayName = null,
            MaterialIconKind? icon = null,
            bool isKeepLive = false,
            bool isTopNavigation = false,
            int priority = 0)
        // 调用基类构造函数，明确传递页面类型为typeof(TPage)
        : base(
            pageType: typeof(TView),
            viewModelType: typeof(TViewModel),
            route: route ?? typeof(TView).Name,
            displayName: displayName,
            icon: icon,
            isKeepLive: isKeepLive,
            isTopNavigation: isTopNavigation,
            priority: priority)
    {
        // 额外校验TPage的有效性，避免传入接口或抽象类
        if (typeof(TView).IsInterface)
        {
            throw new ArgumentException($"页面类型{typeof(TView).Name}不能是接口，请使用具体类");
        }

        if (typeof(TView).IsAbstract)
        {
            throw new ArgumentException($"页面类型{typeof(TView).Name}不能是抽象类，请使用具体实现类");
        }
    }
}

public class RouteInfo<TView> : RouteInfo
{
    /// <summary>
    /// 泛型路由构造函数
    /// 自动将页面类型设置为typeof(TPage)，简化路由创建
    /// </summary>
    /// <param name="route">路由地址（不能为空或空字符串）</param>
    /// <param name="displayName">显示名称（可选）</param>
    /// <param name="icon">图标（可选）</param>
    /// <param name="isKeepLive">是否保活（默认：false）</param>
    /// <param name="isTopNavigation">是否顶级路由（默认：false）</param>
    /// <param name="priority">优先级（默认：0）</param>
    public RouteInfo(
            string? route = null,
            string? displayName = null,
            MaterialIconKind? icon = null,
            bool isKeepLive = false,
            bool isTopNavigation = false,
            int priority = 0)
        // 调用基类构造函数，明确传递页面类型为typeof(TPage)
        : base(
            typeof(TView),
            null,
            route ?? typeof(TView).Name,
            displayName,
            icon,
            isKeepLive,
            isTopNavigation,
            priority)
    {
        // 额外校验TPage的有效性，避免传入接口或抽象类
        if (typeof(TView).IsInterface)
        {
            throw new ArgumentException($"页面类型{typeof(TView).Name}不能是接口，请使用具体类");
        }

        if (typeof(TView).IsAbstract)
        {
            throw new ArgumentException($"页面类型{typeof(TView).Name}不能是抽象类，请使用具体实现类");
        }
    }
}

/// <summary>
/// 路由信息帮助类
/// 集中管理所有路由信息，提供全局可访问的路由列表
/// </summary>
public static class RouteInfoHelper
{
    /// <summary>
    /// 所有路由信息的只读列表
    /// 包含应用中所有页面的路由配置
    /// </summary>
    public static IReadOnlyList<RouteInfo> RouteInfos { get; }

    /// <summary>
    /// 静态构造函数
    /// 初始化路由列表，在程序启动时执行一次
    /// 若路由配置有误（如页面类型无效），会在此阶段抛出异常，提前暴露问题
    /// </summary>
    static RouteInfoHelper()
    {
        // 初始化路由列表
        // 每个路由通过泛型RouteInfo<TPage>创建，自动绑定页面类型
        RouteInfos =
        [
            new RouteInfo<MainWindow, PageCurrentViewModel>("/"),
            // 游戏页面路由
            new RouteInfo<PlayGamePage, PlayGameViewModel>(
                route: nameof(PlayGamePage), // 路由地址使用页面类名，便于维护
                displayName: "开始", // 导航菜单显示文本
                icon: MaterialIconKind.GamepadVariantOutline, // 导航菜单图标
                isTopNavigation: true
            ),
            // 用户页面路由
            new RouteInfo<UserPage, UserPageViewModel>(
                route: nameof(UserPage),
                displayName: "个人",
                icon: MaterialIconKind.AccountOutline, // 导航菜单图标
                isTopNavigation: true
            ),
            new RouteInfo<VersionManagePage, VersionManageViewModel>(
                nameof(VersionManagePage),
                displayName: "版本管理",
                icon: MaterialIconKind.FileDocumentOutline,
                isTopNavigation: true
            ),
            new RouteInfo<VersionDownloadSelectPage, VersionDownloadViewModel>(isTopNavigation: true,
                displayName: AppLang.下载版本, icon: MaterialIconKind.Download),
            new RouteInfo<SettingsPage>(isTopNavigation: true, displayName: "设置", icon: MaterialIconKind.Settings),
        ];
    }
}