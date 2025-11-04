using MFToolkit.Avaloniaui.Routes.Infrastructure.DependencyInjection;
using MFToolkit.Avaloniaui.Routes.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;
using OriginX.Options.RouteOptions;

namespace OriginX.Common.Routes.Extensions;

public static class RouteServiceExtensions
{
    public static IServiceCollection AddRoutes(this IServiceCollection services)
    {

        services.AddRoutingServices(builder =>
        {
            foreach (var routeInfo in RouteInfoHelper.RouteInfos)
            {
                builder.AddRoute(routeInfo.PageType, routeInfo.ViewModelType, routeInfo.Route,
                    routeInfo.IsTopNavigation, routeInfo.IsKeepAlive, routeInfo.Priority);
                RoutingService.Default.RegisterRoute(routeInfo);
            }
        });
        return services;
    }
}