namespace OriginX.Minecraft.Enums;
/// <summary>
/// 存储模式枚举
/// 表示游戏资源的不同存储方式
/// </summary>
public enum StorageMode
{
    /// <summary>
    /// 全局共享模式
    /// 所有版本共享同一套资源文件和配置
    /// </summary>
    GlobalShared,

    /// <summary>
    /// 版本隔离模式
    /// 每个版本拥有独立的资源文件和配置
    /// </summary>
    VersionIsolated,

    /// <summary>
    /// 智能混合模式
    /// 核心资源共享，版本特有资源隔离
    /// </summary>
    SmartHybrid,

    /// <summary>
    /// 完全自定义模式
    /// 用户自定义资源和配置的存储路径
    /// </summary>
    FullCustom
}