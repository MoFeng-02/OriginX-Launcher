using System;
using OriginX.Minecraft.Enums;
using OriginX.Options.MinecraftOptions;

namespace OriginX.Models.Versions;

/// <summary>
/// 游戏版本详情（包含专属存储配置）
/// </summary>
public class VersionInfo
{
    /// <summary>
    /// 版本唯一标识（如 "1.20.1"、"1.19.4-forge-45.0.54"）
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// 版本显示名称（如 "1.20.1 正式版"）
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// 版本类型（正式版/快照版等）
    /// </summary>
    public VersionType Type { get; set; } = VersionType.Release;

    /// <summary>
    /// 加载器类型（纯净版/Forge等）
    /// </summary>
    public LoaderType LoaderType { get; set; } = LoaderType.Vanilla;

    /// <summary>
    /// 存储模式
    /// </summary>
    public StorageMode StorageMode { get; set; }
    
    /// <summary>
    /// 该版本专属的存储配置（核心！关联存储模式和路径）
    /// </summary>
    public StorageOptions StorageOptions { get; set; } = new StorageOptions(); // 每个版本有自己的存储配置

    /// <summary>
    /// 版本发布日期
    /// </summary>
    public DateTime ReleaseDate { get; set; } = DateTime.MinValue;

    /// <summary>
    /// 版本状态（已安装/未安装等）
    /// </summary>
    public VersionStatus Status { get; set; } = VersionStatus.NotInstalled;

    /// <summary>
    /// 游戏图标路径
    /// </summary>
    public string? IconPath { get; set; }

    /// <summary>
    /// 是否收藏
    /// </summary>
    public bool IsFavorite { get; set; }
    
    /// <summary>
    /// 根目录，分组依据，自定义模式下可能为空
    /// </summary>
    public string? RootPath { get; set; }
}