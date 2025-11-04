namespace OriginX.Minecraft.Enums;
/// <summary>
/// 版本类型枚举
/// </summary>
public enum VersionType
{
    /// <summary>
    /// 正式版
    /// </summary>
    Release,
    /// <summary>
    /// 快照版（开发中的测试版本）
    /// </summary>
    Snapshot,
    /// <summary>
    /// 测试版（如预发布版）
    /// </summary>
    Beta,
    /// <summary>
    /// 阿尔法版（早期测试版）
    /// </summary>
    Alpha,
    /// <summary>
    /// 自定义版本（用户自制整合包）
    /// </summary>
    Custom
}