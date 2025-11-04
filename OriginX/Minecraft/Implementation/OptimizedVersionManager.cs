using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using MFToolkit.Abstractions.DependencyInjection;
using OriginX.Extensions.JsonExtensions;
using OriginX.Minecraft.Abstractions;
using OriginX.Minecraft.Enums;
using OriginX.Models.Versions;
using OriginX.Options.MinecraftOptions;

namespace OriginX.Minecraft.Implementation;

/// <summary>
/// 基于本地JSON的版本管理实现
/// </summary>
[Singleton<IVersionManager>]
public class OptimizedVersionManager : IVersionManager
{
    // 基础路径：AppData/Versions/（用于存放所有拆分的JSON文件）
    private readonly string _baseVersionDir;

    // 精简信息文件路径：AppData/version_briefs.json（全局唯一）
    private readonly string _briefsFilePath;

    // 内存缓存：根路径 → 版本列表（避免重复读文件）
    private readonly Dictionary<string, List<VersionInfo>> _versionCache = new();

    // JSON序列化配置（紧凑格式+忽略默认值，减小体积）
    private readonly JsonSerializerOptions _jsonOptions = OriginXJsonContext.Default.Options;

    public OptimizedVersionManager()
    {
        // 初始化目录
        var appDataDir = Path.Combine(AppContext.BaseDirectory, "AppData");
        _baseVersionDir = Path.Combine(appDataDir, "Versions"); // 拆分的版本文件目录
        Directory.CreateDirectory(_baseVersionDir);

        // 初始化精简信息文件
        _briefsFilePath = Path.Combine(appDataDir, "version_briefs.json");
        if (!File.Exists(_briefsFilePath))
        {
            File.WriteAllText(_briefsFilePath, "[]"); // 初始化为空列表
        }
    }

    #region 核心优化：按“存储模式+根路径”生成文件路径

    /// <summary>
    /// 根据存储模式和根路径生成唯一的JSON文件名（避免特殊字符）
    /// </summary>
    private string GetVersionFilePath(StorageMode mode, string rootPath)
    {
        // 对根路径进行哈希处理（避免路径中的特殊字符影响文件名）
        var rootHash = Convert.ToBase64String(System.Security.Cryptography.SHA256.HashData(
                System.Text.Encoding.UTF8.GetBytes(rootPath)))
            .Replace("/", "_")
            .Replace("+", "-")
            .TrimEnd('=');

        // 文件名格式：{模式}_{根路径哈希}.json
        return Path.Combine(_baseVersionDir, $"{mode}_{rootHash}.json");
    }

    /// <summary>
    /// 根据版本信息获取对应的文件路径
    /// </summary>
    private string GetVersionFilePath(VersionInfo version)
    {
        var rootPath = VersionBriefInfo.FromVersionInfo(version).RootPath;
        return GetVersionFilePath(version.StorageOptions.StorageMode, rootPath);
    }

    #endregion

    #region 查（优化：按需加载）

    public async Task<List<VersionInfo>> GetAllVersionsAsync()
    {
        // 全量加载（不常用，尽量避免）：加载所有拆分文件并合并
        var allVersions = new List<VersionInfo>();
        foreach (var file in Directory.EnumerateFiles(_baseVersionDir, "*.json"))
        {
            var versions = await ReadVersionsFromFileAsync(file);
            allVersions.AddRange(versions);
        }

        return allVersions;
    }

    public async Task<VersionInfo?> GetVersionByIdAsync(string versionId)
    {
        // 先查精简信息，定位版本所在的文件
        var briefs = await GetAllBriefsAsync();
        var targetBrief = briefs.FirstOrDefault(b => b.Id == versionId);
        if (targetBrief == null) return null;

        // 加载对应文件，获取完整信息
        var filePath = GetVersionFilePath(targetBrief.StorageMode, targetBrief.RootPath);
        var versionsInFile = await ReadVersionsFromFileAsync(filePath);
        return versionsInFile.FirstOrDefault(v => v.Id == versionId);
    }

    public async Task<List<VersionInfo>> GetVersionsByStorageModeAsync(StorageMode storageMode)
    {
        // 先查精简信息，筛选出目标模式的版本ID和根路径
        var briefs = await GetAllBriefsAsync();
        var targetBriefs = briefs.Where(b => b.StorageMode == storageMode).ToList();
        if (!targetBriefs.Any()) return new List<VersionInfo>();

        // 按根路径分组，加载对应的文件
        var result = new List<VersionInfo>();
        foreach (var group in targetBriefs.GroupBy(b => b.RootPath))
        {
            var filePath = GetVersionFilePath(storageMode, group.Key);
            var versionsInFile = await ReadVersionsFromFileAsync(filePath);
            result.AddRange(versionsInFile.Where(v => group.Select(b => b.Id).Contains(v.Id)));
        }

        return result;
    }

    public async Task<List<VersionGroup>> GetVersionsGroupedByStorageAndRootAsync()
    {
        // 关键优化：只加载精简信息生成分组（不加载完整版本数据）
        var allBriefs = await GetAllBriefsAsync();

        // 按存储模式→根路径分组（用精简信息足够）
        return allBriefs
            .GroupBy(b => b.StorageMode)
            .Select(modeGroup => new VersionGroup
            {
                StorageMode = modeGroup.Key,
                RootPath = modeGroup.First().RootPath, // 主根路径
                RootGroups = modeGroup
                    .GroupBy(b => b.RootPath)
                    .ToDictionary(
                        rootGroup => rootGroup.Key,
                        rootGroup => rootGroup.Select(b =>
                            // 临时转换为VersionInfo（只包含列表展示字段）
                            new VersionInfo
                            {
                                Id = b.Id,
                                DisplayName = b.DisplayName,
                                Status = b.Status,
                                IsFavorite = b.IsFavorite
                            }).ToList()
                    )
            })
            .ToList();
    }

    public async Task<bool> ExistsAsync(string versionId)
    {
        // 查精简信息即可，无需加载完整文件
        var briefs = await GetAllBriefsAsync();
        return briefs.Any(b => b.Id == versionId);
    }

    #endregion

    #region 增（优化：只写对应小文件）

    public async Task AddVersionAsync(VersionInfo version)
    {
        if (version == null || string.IsNullOrWhiteSpace(version.Id))
            throw new ArgumentException("版本信息或ID不能为空～");

        // 检查ID是否已存在（查精简信息）
        if (await ExistsAsync(version.Id))
            throw new InvalidOperationException($"版本ID已存在：{version.Id}");

        // 1. 写入版本到对应的拆分文件
        var filePath = GetVersionFilePath(version);
        var versionsInFile = await ReadVersionsFromFileAsync(filePath);
        versionsInFile.Add(version);
        await WriteVersionsToFileAsync(filePath, versionsInFile);

        // 2. 更新精简信息文件
        var briefs = await GetAllBriefsAsync();
        briefs.Add(VersionBriefInfo.FromVersionInfo(version));
        await WriteBriefsToFileAsync(briefs);

        // 3. 更新缓存
        UpdateCache(filePath, versionsInFile);
    }

    public async Task BulkAddVersionsAsync(IEnumerable<VersionInfo> versions)
    {
        var versionsList = versions.Where(v => !string.IsNullOrWhiteSpace(v.Id)).ToList();
        if (versionsList.Count == 0) return;

        // 按文件路径分组，批量写入（减少IO操作）
        var groupedByFile = versionsList
            .GroupBy(GetVersionFilePath)
            .ToDictionary(g => g.Key, g => g.ToList());

        // 1. 批量写入版本文件
        foreach (var (filePath, versionsInFile) in groupedByFile)
        {
            var existingVersions = await ReadVersionsFromFileAsync(filePath);
            // 过滤已存在的ID
            var newVersions = versionsInFile.Where(v => existingVersions.All(e => e.Id != v.Id)).ToList();
            if (newVersions.Count != 0)
            {
                existingVersions.AddRange(newVersions);
                await WriteVersionsToFileAsync(filePath, existingVersions);
                UpdateCache(filePath, existingVersions);
            }
        }

        // 2. 批量更新精简信息
        var briefs = await GetAllBriefsAsync();
        var existingIds = briefs.Select(b => b.Id).ToHashSet();
        var newBriefs = versionsList
            .Where(v => !existingIds.Contains(v.Id))
            .Select(VersionBriefInfo.FromVersionInfo)
            .ToList();
        if (newBriefs.Any())
        {
            briefs.AddRange(newBriefs);
            await WriteBriefsToFileAsync(briefs);
        }
    }

    #endregion

    #region 改（优化：只更新对应小文件）

    public async Task UpdateVersionAsync(VersionInfo version)
    {
        if (version == null || string.IsNullOrWhiteSpace(version.Id))
            throw new ArgumentException("版本信息或ID不能为空～");

        // 1. 先删除旧版本（可能跨文件，因为存储模式/根路径可能变更）
        var oldVersion = await GetVersionByIdAsync(version.Id);
        if (oldVersion != null)
        {
            var oldFilePath = GetVersionFilePath(oldVersion);
            var oldVersions = await ReadVersionsFromFileAsync(oldFilePath);
            oldVersions.RemoveAll(v => v.Id == version.Id);
            await WriteVersionsToFileAsync(oldFilePath, oldVersions);
            UpdateCache(oldFilePath, oldVersions);
        }
        else
        {
            throw new KeyNotFoundException($"未找到版本：{version.Id}");
        }

        // 2. 新增新版本（写入新的文件路径）
        await AddVersionAsync(version);
    }

    public async Task UpdateVersionStorageOptionsAsync(string versionId, StorageOptions newStorageOptions)
    {
        var version = await GetVersionByIdAsync(versionId)
                      ?? throw new KeyNotFoundException($"未找到版本：{versionId}");

        version.StorageOptions = newStorageOptions;
        await UpdateVersionAsync(version);
    }

    #endregion

    #region 删（优化：只删除对应小文件中的条目）

    public async Task<bool> DeleteVersionAsync(string versionId)
    {
        // 1. 查找版本所在的文件
        var brief = (await GetAllBriefsAsync()).FirstOrDefault(b => b.Id == versionId);
        if (brief == null) return false;

        var filePath = GetVersionFilePath(brief.StorageMode, brief.RootPath);
        var versionsInFile = await ReadVersionsFromFileAsync(filePath);
        var initialCount = versionsInFile.Count;

        // 2. 从文件中删除
        versionsInFile.RemoveAll(v => v.Id == versionId);
        await WriteVersionsToFileAsync(filePath, versionsInFile);
        UpdateCache(filePath, versionsInFile);

        // 3. 从精简信息中删除
        var briefs = await GetAllBriefsAsync();
        if (briefs.RemoveAll(b => b.Id == versionId) > 0)
        {
            await WriteBriefsToFileAsync(briefs);
        }

        return versionsInFile.Count < initialCount;
    }

    public async Task<int> BulkDeleteVersionsAsync(IEnumerable<string> versionIds)
    {
        var idsToDelete = versionIds.Where(id => !string.IsNullOrWhiteSpace(id)).ToHashSet();
        if (idsToDelete.Count == 0) return 0;

        // 1. 按文件分组删除
        var briefs = await GetAllBriefsAsync();
        var briefsToDelete = briefs.Where(b => idsToDelete.Contains(b.Id)).ToList();
        var deletedCount = 0;

        foreach (var group in briefsToDelete.GroupBy(b => GetVersionFilePath(b.StorageMode, b.RootPath)))
        {
            var filePath = group.Key;
            var versionsInFile = await ReadVersionsFromFileAsync(filePath);
            var countBefore = versionsInFile.Count;

            versionsInFile.RemoveAll(v => idsToDelete.Contains(v.Id));
            await WriteVersionsToFileAsync(filePath, versionsInFile);
            UpdateCache(filePath, versionsInFile);

            deletedCount += countBefore - versionsInFile.Count;
        }

        // 2. 从精简信息中删除
        var remainingBriefs = briefs.Where(b => !idsToDelete.Contains(b.Id)).ToList();
        await WriteBriefsToFileAsync(remainingBriefs);

        return deletedCount;
    }

    public async Task ClearAllVersionsAsync()
    {
        // 删除所有拆分的版本文件
        foreach (var file in Directory.EnumerateFiles(_baseVersionDir, "*.json"))
        {
            File.Delete(file);
        }

        // 清空精简信息文件
        await File.WriteAllTextAsync(_briefsFilePath, "[]");

        // 清空缓存
        _versionCache.Clear();
    }

    #endregion

    #region 精简信息文件操作（核心优化点）

    /// <summary>
    /// 读取所有精简信息（快速，用于分组）
    /// </summary>
    private async Task<List<VersionBriefInfo>> GetAllBriefsAsync()
    {
        try
        {
            var json = await File.ReadAllTextAsync(_briefsFilePath);
            return JsonSerializer.Deserialize<List<VersionBriefInfo>>(json, _jsonOptions) ??
                   new List<VersionBriefInfo>();
        }
        catch
        {
            return new List<VersionBriefInfo>();
        }
    }

    /// <summary>
    /// 写入精简信息到文件
    /// </summary>
    private async Task WriteBriefsToFileAsync(List<VersionBriefInfo> briefs)
    {
        var json = JsonSerializer.Serialize(briefs, _jsonOptions);
        await File.WriteAllTextAsync(_briefsFilePath, json);
    }

    #endregion

    #region 拆分的版本文件操作

    /// <summary>
    /// 从文件读取版本列表（带缓存）
    /// </summary>
    private async Task<List<VersionInfo>> ReadVersionsFromFileAsync(string filePath)
    {
        // 先查缓存
        if (_versionCache.TryGetValue(filePath, out var cached))
        {
            return [..cached]; // 返回副本，避免缓存被意外修改
        }

        // 缓存未命中，读文件
        List<VersionInfo> versions;
        if (File.Exists(filePath))
        {
            try
            {
                var json = await File.ReadAllTextAsync(filePath);
                versions = JsonSerializer.Deserialize<List<VersionInfo>>(json, _jsonOptions) ?? [];
            }
            catch
            {
                versions = [];
            }
        }
        else
        {
            versions = [];
        }

        // 更新缓存
        _versionCache[filePath] = versions;
        return [..versions];
    }

    /// <summary>
    /// 写入版本列表到文件（同步更新缓存）
    /// </summary>
    private async Task WriteVersionsToFileAsync(string filePath, List<VersionInfo> versions)
    {
        var json = JsonSerializer.Serialize(versions, _jsonOptions);
        await File.WriteAllTextAsync(filePath, json);
        // 更新缓存
        _versionCache[filePath] = new List<VersionInfo>(versions);
    }

    /// <summary>
    /// 更新缓存（移除空列表的文件缓存，节省内存）
    /// </summary>
    private void UpdateCache(string filePath, List<VersionInfo> versions)
    {
        if (versions.Count == 0)
        {
            _versionCache.Remove(filePath); // 空文件从缓存移除
            // 可选：删除空文件
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
        else
        {
            _versionCache[filePath] = new List<VersionInfo>(versions);
        }
    }

    #endregion
}