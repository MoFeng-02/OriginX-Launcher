namespace OriginX.Minecraft.Enums;

/// <summary>
/// 新增：硬件加速模式枚举
/// </summary>
public enum HardwareAccelerationMode
{
    /// <summary>
    /// 自动检测（默认）
    /// </summary>
    Auto,
    /// <summary>
    /// 强制启用
    /// </summary>
    Enabled,
    /// <summary>
    /// 强制禁用（用于解决兼容性问题）
    /// </summary>
    Disabled
}