using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OriginX.Options.MinecraftOptions;

namespace OriginX.Configurations;

/// <summary>
/// 提供Options注入
/// </summary>
public static class OptionsConfiguration
{
    public static IServiceCollection AddOptionsConfiguration(this IServiceCollection services)
    {
        // 配置文件路径（默认与程序同目录）
        var basePath = Path.Combine(AppContext.BaseDirectory, "Options");
        if (!Directory.Exists(basePath))
        {
            Directory.CreateDirectory(basePath);
        }
        var configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile($"{nameof(StorageOptions)}.json", optional: true, reloadOnChange: true)
            .AddJsonFile($"{nameof(LaunchOptions)}.json", optional: true, reloadOnChange: true)
            .Build();

        services.Configure<StorageOptions>(configuration.GetSection(nameof(StorageOptions)));
        services.Configure<LaunchOptions>(configuration.GetSection(nameof(LaunchOptions)));
        return services;
    }
}