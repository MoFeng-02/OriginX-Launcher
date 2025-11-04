using System;
using System.IO;
using MFToolkit.Abstractions.DependencyInjection;
using OriginX.Minecraft.Enums;

namespace OriginX.Options.MinecraftOptions;

/// <summary>
/// Minecraft 存储相关的配置选项
/// 管理所有游戏内容（资源、存档、MOD等）的存储方式和路径
/// </summary>
public class StorageOptions
{
    /// <summary>
    /// 存储模式（全局共享/版本隔离/智能混合/完全自定义）
    /// </summary>
    public StorageMode StorageMode { get; set; } = StorageMode.SmartHybrid;

    #region 基础路径（全局/版本隔离共用）
    private string? _globalRoot;
    /// <summary>
    /// 全局共享存储根目录（GlobalShared/SmartHybrid 模式用）
    /// 默认是系统默认的 .minecraft 文件夹，不会为 null
    /// </summary>
    public string GlobalRoot
    {
        get => _globalRoot ??= Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            ".minecraft"
        );
        set => _globalRoot = string.IsNullOrWhiteSpace(value) ? null : value;
    }

    private string? _versionRoot;
    /// <summary>
    /// 版本隔离存储根目录（VersionIsolated/SmartHybrid 模式用）
    /// 为空时会自动回退到全局根目录
    /// </summary>
    public string? VersionRoot
    {
        get => _versionRoot;
        set => _versionRoot = string.IsNullOrWhiteSpace(value) ? null : value;
    }
    #endregion

    #region 自定义模式专属路径（FullCustom 模式用）
    private string? _customAssetsPath;
    /// <summary>
    /// 自定义资源文件路径（assets）
    /// FullCustom 模式下必须设置有效值
    /// </summary>
    public string? CustomAssetsPath
    {
        get => _customAssetsPath;
        set => _customAssetsPath = string.IsNullOrWhiteSpace(value) ? null : value;
    }

    private string? _customConfigPath;
    /// <summary>
    /// 自定义配置文件路径（config）
    /// FullCustom 模式下必须设置有效值
    /// </summary>
    public string? CustomConfigPath
    {
        get => _customConfigPath;
        set => _customConfigPath = string.IsNullOrWhiteSpace(value) ? null : value;
    }

    private string? _customSavesPath;
    /// <summary>
    /// 自定义存档路径（saves）
    /// FullCustom 模式下必须设置有效值
    /// </summary>
    public string? CustomSavesPath
    {
        get => _customSavesPath;
        set => _customSavesPath = string.IsNullOrWhiteSpace(value) ? null : value;
    }

    private string? _customModsPath;
    /// <summary>
    /// 自定义MOD路径（mods）
    /// FullCustom 模式下必须设置有效值
    /// </summary>
    public string? CustomModsPath
    {
        get => _customModsPath;
        set => _customModsPath = string.IsNullOrWhiteSpace(value) ? null : value;
    }

    private string? _customShadersPath;
    /// <summary>
    /// 自定义光影路径（shaderpacks）
    /// FullCustom 模式下必须设置有效值
    /// </summary>
    public string? CustomShadersPath
    {
        get => _customShadersPath;
        set => _customShadersPath = string.IsNullOrWhiteSpace(value) ? null : value;
    }

    private string? _customResourcePacksPath;
    /// <summary>
    /// 自定义材质包路径（resourcepacks）
    /// FullCustom 模式下必须设置有效值
    /// </summary>
    public string? CustomResourcePacksPath
    {
        get => _customResourcePacksPath;
        set => _customResourcePacksPath = string.IsNullOrWhiteSpace(value) ? null : value;
    }

    private string? _customLogsPath;
    /// <summary>
    /// 自定义日志路径（logs）
    /// 所有模式下设置后都会覆盖默认路径
    /// </summary>
    public string? CustomLogsPath
    {
        get => _customLogsPath;
        set => _customLogsPath = string.IsNullOrWhiteSpace(value) ? null : value;
    }

    private string? _customScreenshotsPath;
    /// <summary>
    /// 自定义截图路径（screenshots）
    /// FullCustom 模式下必须设置有效值
    /// </summary>
    public string? CustomScreenshotsPath
    {
        get => _customScreenshotsPath;
        set => _customScreenshotsPath = string.IsNullOrWhiteSpace(value) ? null : value;
    }

    private string? _customReplaysPath;
    /// <summary>
    /// 自定义录像路径（replays，如使用ReplayMod）
    /// FullCustom 模式下必须设置有效值
    /// </summary>
    public string? CustomReplaysPath
    {
        get => _customReplaysPath;
        set => _customReplaysPath = string.IsNullOrWhiteSpace(value) ? null : value;
    }
    #endregion

    #region 通用配置
    /// <summary>
    /// 是否自动创建不存在的文件夹
    /// 比如启动时发现没有saves文件夹，自动建一个
    /// </summary>
    public bool AutoCreateFolders { get; set; } = true;

    /// <summary>
    /// 是否启用资源文件校验
    /// 启动时检查assets文件的哈希值是否正确，防止损坏
    /// </summary>
    public bool ValidateResources { get; set; } = true;

    /// <summary>
    /// 缓存文件保留天数（SmartHybrid模式用）
    /// 超过这个天数的临时缓存（如旧版本资产）会被自动清理
    /// 最小值为1天
    /// </summary>
    private int _cacheRetentionDays = 30;
    public int CacheRetentionDays
    {
        get => _cacheRetentionDays;
        set => _cacheRetentionDays = Math.Max(1, value); // 确保不会小于1
    }

    /// <summary>
    /// 是否允许跨版本共享材质包（SmartHybrid模式用）
    /// 开启后不同版本可以共用同一个材质包文件夹
    /// </summary>
    public bool AllowCrossVersionResourcePacks { get; set; } = true;

    /// <summary>
    /// 是否将截图自动备份到全局目录（VersionIsolated模式用）
    /// 防止删除版本文件夹时丢失截图
    /// </summary>
    public bool BackupScreenshotsToGlobal { get; set; } = false;
    #endregion

    #region 路径计算（根据当前模式返回实际路径，确保非null）
    /// <summary>
    /// 获取实际的资源文件路径（assets），不会为null
    /// </summary>
    public string GetAssetsPath()
    {
        return StorageMode switch
        {
            StorageMode.GlobalShared => Path.Combine(GlobalRoot, "assets"),
            StorageMode.VersionIsolated => Path.Combine(VersionRoot ?? GlobalRoot, "assets"),
            StorageMode.SmartHybrid => Path.Combine(GlobalRoot, "assets"),
            StorageMode.FullCustom => EnsureCustomPath(CustomAssetsPath, "资源文件(assets)"),
            _ => throw new ArgumentOutOfRangeException(nameof(StorageMode), StorageMode, "未知的存储模式")
        };
    }

    /// <summary>
    /// 获取实际的配置文件路径（config），不会为null
    /// </summary>
    public string GetConfigPath()
    {
        return StorageMode switch
        {
            StorageMode.GlobalShared => Path.Combine(GlobalRoot, "config"),
            StorageMode.VersionIsolated => Path.Combine(VersionRoot ?? GlobalRoot, "config"),
            StorageMode.SmartHybrid => Path.Combine(VersionRoot ?? GlobalRoot, "config"),
            StorageMode.FullCustom => EnsureCustomPath(CustomConfigPath, "配置文件(config)"),
            _ => throw new ArgumentOutOfRangeException(nameof(StorageMode), StorageMode, "未知的存储模式")
        };
    }

    /// <summary>
    /// 获取实际的存档路径（saves），不会为null
    /// </summary>
    public string GetSavesPath()
    {
        return StorageMode switch
        {
            StorageMode.GlobalShared => Path.Combine(GlobalRoot, "saves"),
            StorageMode.VersionIsolated => Path.Combine(VersionRoot ?? GlobalRoot, "saves"),
            StorageMode.SmartHybrid => Path.Combine(GlobalRoot, "saves"),
            StorageMode.FullCustom => EnsureCustomPath(CustomSavesPath, "存档(saves)"),
            _ => throw new ArgumentOutOfRangeException(nameof(StorageMode), StorageMode, "未知的存储模式")
        };
    }

    /// <summary>
    /// 获取实际的MOD路径（mods），不会为null
    /// </summary>
    public string GetModsPath()
    {
        return StorageMode switch
        {
            StorageMode.GlobalShared => Path.Combine(GlobalRoot, "mods"),
            StorageMode.VersionIsolated => Path.Combine(VersionRoot ?? GlobalRoot, "mods"),
            StorageMode.SmartHybrid => Path.Combine(VersionRoot ?? GlobalRoot, "mods"),
            StorageMode.FullCustom => EnsureCustomPath(CustomModsPath, "MOD(mods)"),
            _ => throw new ArgumentOutOfRangeException(nameof(StorageMode), StorageMode, "未知的存储模式")
        };
    }

    /// <summary>
    /// 获取实际的光影路径（shaderpacks），不会为null
    /// </summary>
    public string GetShadersPath()
    {
        return StorageMode switch
        {
            StorageMode.GlobalShared => Path.Combine(GlobalRoot, "shaderpacks"),
            StorageMode.VersionIsolated => Path.Combine(VersionRoot ?? GlobalRoot, "shaderpacks"),
            StorageMode.SmartHybrid => Path.Combine(GlobalRoot, "shaderpacks"),
            StorageMode.FullCustom => EnsureCustomPath(CustomShadersPath, "光影(shaderpacks)"),
            _ => throw new ArgumentOutOfRangeException(nameof(StorageMode), StorageMode, "未知的存储模式")
        };
    }

    /// <summary>
    /// 获取实际的材质包路径（resourcepacks），不会为null
    /// </summary>
    public string GetResourcePacksPath()
    {
        return StorageMode switch
        {
            StorageMode.GlobalShared => Path.Combine(GlobalRoot, "resourcepacks"),
            StorageMode.VersionIsolated => Path.Combine(VersionRoot ?? GlobalRoot, "resourcepacks"),
            StorageMode.SmartHybrid => AllowCrossVersionResourcePacks
                ? Path.Combine(GlobalRoot, "resourcepacks")
                : Path.Combine(VersionRoot ?? GlobalRoot, "resourcepacks"),
            StorageMode.FullCustom => EnsureCustomPath(CustomResourcePacksPath, "材质包(resourcepacks)"),
            _ => throw new ArgumentOutOfRangeException(nameof(StorageMode), StorageMode, "未知的存储模式")
        };
    }

    /// <summary>
    /// 获取实际的日志路径（logs），不会为null
    /// </summary>
    public string GetLogsPath()
    {
        // 自定义日志路径优先级最高，非null时直接使用
        if (CustomLogsPath is not null)
            return CustomLogsPath;

        return StorageMode switch
        {
            StorageMode.GlobalShared => Path.Combine(GlobalRoot, "logs"),
            StorageMode.VersionIsolated => Path.Combine(VersionRoot ?? GlobalRoot, "logs"),
            StorageMode.SmartHybrid => Path.Combine(GlobalRoot, "logs"),
            StorageMode.FullCustom => EnsureCustomPath(CustomLogsPath, "日志(logs)"),
            _ => throw new ArgumentOutOfRangeException(nameof(StorageMode), StorageMode, "未知的存储模式")
        };
    }

    /// <summary>
    /// 获取实际的截图路径（screenshots），不会为null
    /// </summary>
    public string GetScreenshotsPath()
    {
        return StorageMode switch
        {
            StorageMode.GlobalShared => Path.Combine(GlobalRoot, "screenshots"),
            StorageMode.VersionIsolated => Path.Combine(VersionRoot ?? GlobalRoot, "screenshots"),
            StorageMode.SmartHybrid => Path.Combine(GlobalRoot, "screenshots"),
            StorageMode.FullCustom => EnsureCustomPath(CustomScreenshotsPath, "截图(screenshots)"),
            _ => throw new ArgumentOutOfRangeException(nameof(StorageMode), StorageMode, "未知的存储模式")
        };
    }

    /// <summary>
    /// 获取实际的录像路径（replays），不会为null
    /// </summary>
    public string GetReplaysPath()
    {
        return StorageMode switch
        {
            StorageMode.GlobalShared => Path.Combine(GlobalRoot, "replays"),
            StorageMode.VersionIsolated => Path.Combine(VersionRoot ?? GlobalRoot, "replays"),
            StorageMode.SmartHybrid => Path.Combine(GlobalRoot, "replays"),
            StorageMode.FullCustom => EnsureCustomPath(CustomReplaysPath, "录像(replays)"),
            _ => throw new ArgumentOutOfRangeException(nameof(StorageMode), StorageMode, "未知的存储模式")
        };
    }
    #endregion

    #region 辅助方法（强化null安全）
    /// <summary>
    /// 确保自定义路径已设置（否则抛异常），返回非null路径
    /// </summary>
    private string EnsureCustomPath(string? path, string displayName)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new InvalidOperationException($"完全自定义模式下，必须设置有效的{displayName}路径哦～");
        return path;
    }

    /// <summary>
    /// 初始化所有存储路径（自动创建文件夹，处理null路径）
    /// </summary>
    public void InitializeAllPaths()
    {
        if (!AutoCreateFolders) return;

        try
        {
            // 批量创建所有必要文件夹（确保路径非null）
            CreateDirectoryIfMissing(GetAssetsPath());
            CreateDirectoryIfMissing(GetConfigPath());
            CreateDirectoryIfMissing(GetSavesPath());
            CreateDirectoryIfMissing(GetModsPath());
            CreateDirectoryIfMissing(GetShadersPath());
            CreateDirectoryIfMissing(GetResourcePacksPath());
            CreateDirectoryIfMissing(GetLogsPath());
            CreateDirectoryIfMissing(GetScreenshotsPath());
            CreateDirectoryIfMissing(GetReplaysPath());

            // 版本隔离模式下的截图备份目录
            if (StorageMode == StorageMode.VersionIsolated && BackupScreenshotsToGlobal)
            {
                var globalBackupPath = Path.Combine(GlobalRoot, "screenshots_backup");
                CreateDirectoryIfMissing(globalBackupPath);
            }
        }
        catch (Exception ex)
        {
            throw new IOException("初始化存储路径时发生错误～", ex);
        }
    }

    /// <summary>
    /// 安全创建文件夹（处理null和无效路径）
    /// </summary>
    private void CreateDirectoryIfMissing(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentNullException(nameof(path), "文件夹路径不能为null或空字符串～");

        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
    }

    /// <summary>
    /// 清理过期缓存（仅SmartHybrid模式，安全处理路径）
    /// </summary>
    public void CleanupExpiredCache()
    {
        if (StorageMode != StorageMode.SmartHybrid) return;

        var cacheDir = Path.Combine(GlobalRoot, "cache");
        if (!Directory.Exists(cacheDir)) return;

        var cutoffTime = DateTime.Now.AddDays(-CacheRetentionDays);
        foreach (var file in Directory.EnumerateFiles(cacheDir, "*.*", SearchOption.AllDirectories))
        {
            try
            {
                if (File.GetLastWriteTimeUtc(file) < cutoffTime)
                {
                    File.Delete(file);
                }
            }
            catch (IOException)
            {
                // 忽略正在使用的文件
            }
            catch (UnauthorizedAccessException)
            {
                // 忽略无权限的文件
            }
        }
    }
    #endregion
}