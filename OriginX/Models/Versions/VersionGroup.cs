using System.Collections.Generic;
using OriginX.Minecraft.Enums;

namespace OriginX.Models.Versions;

/// <summary>
/// 按存储模式和根目录分组的版本集合
/// </summary>
public class VersionGroup
{
    /// <summary>
    /// 存储模式（全局/隔离/混合/自定义）
    /// </summary>
    public StorageMode StorageMode { get; set; }

    /// <summary>
    /// 根目录，分组依据，自定义模式下可能为空
    /// </summary>
    public string? RootPath { get; set; }

    /// <summary>
    /// 该模式下的根目录分组（key：根目录路径，value：使用该根目录的版本列表）
    /// </summary>
    public Dictionary<string, List<VersionInfo>> RootGroups { get; set; } = [];
}