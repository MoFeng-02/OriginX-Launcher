using System;
using System.Collections.Generic;
using System.IO;
using OriginX.Minecraft.Enums;
using OriginX.Options.MinecraftOptions;

namespace OriginX.Models.Versions;

/// <summary>
/// 版本精简信息（仅包含分组和列表展示所需字段，体积小）
/// </summary>
public class VersionBriefInfo
{
    /// <summary>
    /// 版本唯一标识（与VersionInfo一致）
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// 版本显示名称
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// 存储模式（用于分组）
    /// </summary>
    public StorageMode StorageMode { get; set; }

    /// <summary>
    /// 根目录路径（用于分组）
    /// </summary>
    public string RootPath { get; set; } = string.Empty;

    /// <summary>
    /// 版本状态（用于列表展示）
    /// </summary>
    public VersionStatus Status { get; set; }

    /// <summary>
    /// 是否收藏（用于排序）
    /// </summary>
    public bool IsFavorite { get; set; }

    /// <summary>
    /// 从完整版本信息转换为精简信息
    /// </summary>
    public static VersionBriefInfo FromVersionInfo(VersionInfo version)
    {
        // 获取版本的实际根目录（复用之前的路径计算逻辑）
        var rootPath = version.StorageMode switch
        {
            StorageMode.GlobalShared or StorageMode.SmartHybrid => version.StorageOptions.GlobalRoot,
            StorageMode.VersionIsolated => version.StorageOptions.VersionRoot ?? version.StorageOptions.GlobalRoot,
            StorageMode.FullCustom => GetCustomRoot(version.StorageOptions),
            _ => version.StorageOptions.GlobalRoot
        };

        return new VersionBriefInfo
        {
            Id = version.Id,
            DisplayName = version.DisplayName,
            StorageMode = version.StorageOptions.StorageMode,
            RootPath = Path.GetFullPath(rootPath),
            Status = version.Status,
            IsFavorite = version.IsFavorite
        };
    }

    // 复用自定义模式根目录计算逻辑
    private static string GetCustomRoot(StorageOptions storageOptions)
    {
        var priorityPaths = new List<string?>
        {
            storageOptions.CustomAssetsPath,
            storageOptions.CustomSavesPath,
            storageOptions.CustomModsPath,
            storageOptions.GlobalRoot
        };

        foreach (var path in priorityPaths)
        {
            if (!string.IsNullOrWhiteSpace(path))
            {
                return Path.GetFullPath(path);
            }
        }

        return storageOptions.GlobalRoot;
    }
}