namespace OriginX.Minecraft.Enums;


/// <summary>
/// 版本状态枚举
/// </summary>
public enum VersionStatus
{
    /// <summary>
    /// 未安装
    /// </summary>
    NotInstalled,
    /// <summary>
    /// 已安装
    /// </summary>
    Installed,
    /// <summary>
    /// 有更新
    /// </summary>
    UpdateAvailable,
    /// <summary>
    /// 安装中
    /// </summary>
    Installing,
    /// <summary>
    /// 安装失败
    /// </summary>
    InstallFailed
}