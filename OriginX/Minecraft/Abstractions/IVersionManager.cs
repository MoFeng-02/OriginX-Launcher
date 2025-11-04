using System.Collections.Generic;
using System.Threading.Tasks;
using OriginX.Minecraft.Enums;
using OriginX.Models.Versions;
using OriginX.Options.MinecraftOptions;

namespace OriginX.Minecraft.Abstractions;
/// <summary>
/// 版本管理服务接口（基于本地JSON存储）
/// </summary>
public interface IVersionManager
{
    #region 查（获取版本）
    /// <summary>
    /// 获取所有版本（全量列表）
    /// </summary>
    Task<List<VersionInfo>> GetAllVersionsAsync();

    /// <summary>
    /// 按ID获取单个版本
    /// </summary>
    /// <param name="versionId">版本ID</param>
    /// <returns>版本信息（不存在返回null）</returns>
    Task<VersionInfo?> GetVersionByIdAsync(string versionId);

    /// <summary>
    /// 按存储模式筛选版本
    /// </summary>
    Task<List<VersionInfo>> GetVersionsByStorageModeAsync(StorageMode storageMode);

    /// <summary>
    /// 按存储模式+根目录分组
    /// </summary>
    Task<List<VersionGroup>> GetVersionsGroupedByStorageAndRootAsync();

    /// <summary>
    /// 检查版本ID是否已存在
    /// </summary>
    Task<bool> ExistsAsync(string versionId);
    #endregion

    #region 增（添加版本）
    /// <summary>
    /// 添加新版本（ID不存在时才添加）
    /// </summary>
    /// <exception cref="InvalidOperationException">ID已存在时抛出</exception>
    Task AddVersionAsync(VersionInfo version);

    /// <summary>
    /// 批量添加版本（自动跳过已存在的ID）
    /// </summary>
    Task BulkAddVersionsAsync(IEnumerable<VersionInfo> versions);
    #endregion

    #region 改（更新版本）
    /// <summary>
    /// 更新版本信息（ID必须存在）
    /// </summary>
    /// <exception cref="KeyNotFoundException">ID不存在时抛出</exception>
    Task UpdateVersionAsync(VersionInfo version);

    /// <summary>
    /// 仅更新版本的存储配置
    /// </summary>
    /// <exception cref="KeyNotFoundException">ID不存在时抛出</exception>
    Task UpdateVersionStorageOptionsAsync(string versionId, StorageOptions newStorageOptions);
    #endregion

    #region 删（删除版本）
    /// <summary>
    /// 按ID删除版本
    /// </summary>
    /// <returns>是否删除成功（ID不存在返回false）</returns>
    Task<bool> DeleteVersionAsync(string versionId);

    /// <summary>
    /// 批量删除版本
    /// </summary>
    /// <returns>成功删除的数量</returns>
    Task<int> BulkDeleteVersionsAsync(IEnumerable<string> versionIds);

    /// <summary>
    /// 清空所有版本（危险操作！）
    /// </summary>
    Task ClearAllVersionsAsync();
    #endregion
}