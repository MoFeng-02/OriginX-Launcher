using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using MFToolkit.Abstractions.DependencyInjection;
using OriginX.Minecraft.Enums;

namespace OriginX.Options.MinecraftOptions;

/// <summary>
/// Minecraft 启动相关的配置选项
/// 包含游戏启动所需的完整配置，支持自动内存分配、模组管理、性能优化等
/// </summary>
public class LaunchOptions
{
    #region 新增：模组与插件控制
    /// <summary>
    /// 是否启用模组加载（仅客户端/forge/fabric等生效）
    /// 关闭后会忽略 mods 文件夹中的所有模组
    /// </summary>
    public bool EnableMods { get; set; } = true;

    /// <summary>
    /// 强制启用的模组列表（即使文件名包含禁用标记，如 .disabled）
    /// 填写模组文件名（不含路径，如 "modmenu-1.20.1.jar"）
    /// </summary>
    public List<string> ForceEnabledMods { get; } = new();

    /// <summary>
    /// 强制禁用的模组列表（即使在 mods 文件夹中）
    /// 填写模组文件名（不含路径，如 "optifine.jar"）
    /// </summary>
    public List<string> ForceDisabledMods { get; } = new();

    /// <summary>
    /// 启动前是否自动清理失效模组（如版本不兼容的mod）
    /// 仅在 EnableMods 为 true 时生效
    /// </summary>
    public bool CleanInvalidModsOnLaunch { get; set; } = false;
    #endregion

    #region 新增：启动前准备工作
    /// <summary>
    /// 启动前是否验证游戏文件完整性（检查损坏/缺失的核心文件）
    /// </summary>
    public bool VerifyGameFilesBeforeLaunch { get; set; } = false;

    /// <summary>
    /// 启动前是否清理旧日志文件（保留最近N天的日志）
    /// </summary>
    public bool CleanOldLogsBeforeLaunch { get; set; } = true;

    /// <summary>
    /// 日志文件保留天数（配合 CleanOldLogsBeforeLaunch 使用）
    /// </summary>
    public int LogRetentionDays { get; set; } = 7;

    /// <summary>
    /// 启动前执行的自定义命令（如备份存档脚本）
    /// 为空时不执行，支持跨平台命令（Windows用cmd，Linux/macOS用bash）
    /// </summary>
    public string? PreLaunchCommand { get; set; }

    /// <summary>
    /// 自定义命令超时时间（秒）
    /// 超过此时长未执行完则强制终止
    /// </summary>
    public int PreLaunchCommandTimeoutSeconds { get; set; } = 30;
    #endregion

    #region 新增：性能优化选项
    /// <summary>
    /// 是否启用快速启动模式（跳过部分非必要检查）
    /// 可能略微提高启动速度，但降低错误检测能力
    /// </summary>
    public bool FastLaunchMode { get; set; } = false;

    /// <summary>
    /// 是否启用硬件加速渲染（如OpenGL/Vulkan）
    /// 默认为自动检测，关闭后可能降低帧率但提高兼容性
    /// </summary>
    public HardwareAccelerationMode HardwareAcceleration { get; set; } = HardwareAccelerationMode.Auto;

    /// <summary>
    /// 最大线程数限制（0表示使用系统默认）
    /// 用于限制游戏使用的CPU核心数量，避免占用过高
    /// </summary>
    public int MaxThreads { get; set; } = 0;
    #endregion

    #region 版本核心信息（已有，保持不变）
    private string? _versionId;
    public string? VersionId
    {
        get => _versionId;
        set => _versionId = string.IsNullOrWhiteSpace(value) ? null : value;
    }

    private string? _mainClass;
    public string? MainClass
    {
        get => _mainClass;
        set => _mainClass = string.IsNullOrWhiteSpace(value) ? null : value;
    }

    public GameType GameType { get; set; } = GameType.Client;
    #endregion

    #region 路径配置（已有，保持不变）
    private string? _gameRootPath;
    public string? GameRootPath
    {
        get => _gameRootPath;
        set => _gameRootPath = string.IsNullOrWhiteSpace(value) ? null : value;
    }

    private string? _assetsPath;
    public string? AssetsPath
    {
        get => _assetsPath;
        set => _assetsPath = string.IsNullOrWhiteSpace(value) ? null : value;
    }

    private string? _librariesPath;
    public string? LibrariesPath
    {
        get => _librariesPath;
        set => _librariesPath = string.IsNullOrWhiteSpace(value) ? null : value;
    }

    private string? _versionJarPath;
    public string? VersionJarPath
    {
        get => _versionJarPath;
        set => _versionJarPath = string.IsNullOrWhiteSpace(value) ? null : value;
    }
    #endregion

    #region 登录信息（已有，保持不变）
    private string? _username;
    public string Username
    {
        get => _username ??= "Player" + Random.Shared.Next(1000, 9999);
        set => _username = string.IsNullOrWhiteSpace(value) ? null : value;
    }

    private string? _uuid;
    public string? Uuid
    {
        get => _uuid;
        set => _uuid = string.IsNullOrWhiteSpace(value) ? null : value;
    }

    private string? _accessToken;
    public string? AccessToken
    {
        get => _accessToken;
        set => _accessToken = string.IsNullOrWhiteSpace(value) ? null : value;
    }

    public bool OfflineMode { get; set; } = true;
    #endregion

    #region 内存配置（已有，保持不变）
    public bool AutoAllocateMemory { get; set; } = true;

    private int _minMemoryMB = 512;
    public int MinMemoryMB
    {
        get => _minMemoryMB;
        set => _minMemoryMB = Math.Max(256, value);
    }

    private int _maxMemoryMB = 2048;
    public int MaxMemoryMB
    {
        get => _maxMemoryMB;
        set => _maxMemoryMB = Math.Max(Math.Max(512, value), MinMemoryMB);
    }

    private double _maxMemoryRatio = 0.3;
    public double MaxMemoryRatio
    {
        get => _maxMemoryRatio;
        set => _maxMemoryRatio = Math.Clamp(value, 0.1, 0.5);
    }

    public int? MemoryPageSizeMB { get; set; }

    public int GetEffectiveMinMemoryMB()
    {
        return AutoAllocateMemory ? CalculateAutoMinMemory() : MinMemoryMB;
    }

    public int GetEffectiveMaxMemoryMB()
    {
        return AutoAllocateMemory ? CalculateAutoMaxMemory() : MaxMemoryMB;
    }

    private int CalculateAutoMinMemory()
    {
        var totalMemoryGB = GetTotalPhysicalMemoryGB();
        return totalMemoryGB switch
        {
            <= 4 => 1024,
            <= 8 => 2048,
            _ => 3072
        };
    }

    private int CalculateAutoMaxMemory()
    {
        var totalMemoryMB = (int)(GetTotalPhysicalMemoryGB() * 1024);
        var maxByRatio = (int)(totalMemoryMB * MaxMemoryRatio);
        var maxLimit = GameType == GameType.Client ? 8192 : 16384;
        return Math.Clamp(maxByRatio, GetEffectiveMinMemoryMB(), maxLimit);
    }

    private double GetTotalPhysicalMemoryGB()
    {
        try
        {
            if (OperatingSystem.IsWindows()) return GetWindowsTotalMemoryGB();
            if (OperatingSystem.IsLinux()) return GetLinuxTotalMemoryGB();
            if (OperatingSystem.IsMacOS()) return GetMacTotalMemoryGB();
        }
        catch { }
        return 8.0;
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool GlobalMemoryStatusEx(ref MEMORYSTATUSEX lpBuffer);

    [StructLayout(LayoutKind.Sequential)]
    private struct MEMORYSTATUSEX
    {
        public uint dwLength;
        public uint dwMemoryLoad;
        public ulong ullTotalPhys;
        public ulong ullAvailPhys;
        public ulong ullTotalPageFile;
        public ulong ullAvailPageFile;
        public ulong ullTotalVirtual;
        public ulong ullAvailVirtual;
        public ulong ullAvailExtendedVirtual;
    }

    private double GetWindowsTotalMemoryGB()
    {
        var memStatus = new MEMORYSTATUSEX { dwLength = (uint)Marshal.SizeOf<MEMORYSTATUSEX>() };
        return GlobalMemoryStatusEx(ref memStatus)
            ? Math.Round(memStatus.ullTotalPhys / (1024.0 * 1024.0 * 1024.0), 1)
            : 8.0;
    }

    private double GetLinuxTotalMemoryGB()
    {
        const string path = "/proc/meminfo";
        if (File.Exists(path))
        {
            foreach (var line in File.ReadLines(path))
            {
                if (line.StartsWith("MemTotal:", StringComparison.OrdinalIgnoreCase) &&
                    line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries) is { Length: >= 2 } parts &&
                    ulong.TryParse(parts[1], out var totalKB))
                {
                    return Math.Round(totalKB / (1024.0 * 1024.0), 1);
                }
            }
        }
        return 8.0;
    }

    private double GetMacTotalMemoryGB()
    {
        const string sysctl = "/usr/sbin/sysctl";
        if (File.Exists(sysctl))
        {
            using var process = new System.Diagnostics.Process
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo(sysctl, "-n hw.memsize")
                {
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            process.Start();
            var output = process.StandardOutput.ReadToEnd().Trim();
            process.WaitForExit();
            if (ulong.TryParse(output, out var totalBytes))
            {
                return Math.Round(totalBytes / (1024.0 * 1024.0 * 1024.0), 1);
            }
        }
        return 8.0;
    }
    #endregion

    #region 窗口配置（已有，保持不变）
    private int _windowWidth = 854;
    public int WindowWidth
    {
        get => _windowWidth;
        set => _windowWidth = Math.Max(640, value);
    }

    private int _windowHeight = 480;
    public int WindowHeight
    {
        get => _windowHeight;
        set => _windowHeight = Math.Max(480, value);
    }

    public bool IsFullscreen { get; set; } = false;
    public bool BorderlessWindow { get; set; } = false;
    public string? WindowTitle { get; set; }
    #endregion

    #region Java配置（已有，保持不变）
    private string? _javaPath;
    public string? JavaPath
    {
        get => _javaPath;
        set => _javaPath = string.IsNullOrWhiteSpace(value) ? null : value;
    }

    public string? RequiredJavaVersion { get; set; }

    private readonly List<string> _jvmArguments = new();
    public IReadOnlyList<string> JvmArguments => _jvmArguments.AsReadOnly();

    public void AddJvmArgument(string? argument)
    {
        if (!string.IsNullOrWhiteSpace(argument)) _jvmArguments.Add(argument);
    }

    public void AddJvmArguments(IEnumerable<string?>? arguments)
    {
        if (arguments != null) foreach (var arg in arguments) AddJvmArgument(arg);
    }
    #endregion

    #region 游戏参数（已有，保持不变）
    private readonly List<string> _gameArguments = new();
    public IReadOnlyList<string> GameArguments => _gameArguments.AsReadOnly();

    public void AddGameArgument(string? argument)
    {
        if (!string.IsNullOrWhiteSpace(argument)) _gameArguments.Add(argument);
    }

    public void AddGameArguments(IEnumerable<string?>? arguments)
    {
        if (arguments != null) foreach (var arg in arguments) AddGameArgument(arg);
    }
    #endregion

    #region 调试与日志（已有，保持不变）
    public bool EnableDebug { get; set; } = false;

    private string? _logFilePath;
    public string? LogFilePath
    {
        get => _logFilePath;
        set => _logFilePath = string.IsNullOrWhiteSpace(value) ? null : value;
    }

    public bool LogToConsole { get; set; } = true;
    #endregion

    #region 辅助方法（新增部分逻辑）
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(VersionId))
            throw new InvalidOperationException("必须设置有效的游戏版本标识（VersionId）");

        if (string.IsNullOrWhiteSpace(MainClass))
            throw new InvalidOperationException("必须设置有效的游戏主类（MainClass）");

        if (OfflineMode && string.IsNullOrWhiteSpace(Uuid))
            Uuid = GenerateOfflineUuid(Username);

        if (!OfflineMode && string.IsNullOrWhiteSpace(AccessToken))
            throw new InvalidOperationException("在线模式下必须设置有效的访问令牌（AccessToken）");

        // 新增：验证预启动命令是否存在（如果设置了）
        if (!string.IsNullOrWhiteSpace(PreLaunchCommand) && GameType == GameType.Client)
        {
            // 简单检查命令格式（比如是否包含可执行文件）
            var cmdParts = PreLaunchCommand.Split(' ');
            if (cmdParts.Length > 0 && !File.Exists(cmdParts[0]) &&
                !OperatingSystem.IsWindows() && !PreLaunchCommand.StartsWith("/"))
            {
                // 非Windows系统下检查是否是系统命令（如bash、rm等）
                var systemPaths = Environment.GetEnvironmentVariable("PATH")?.Split(Path.PathSeparator) ?? Array.Empty<string>();
                if (!systemPaths.Any(p => File.Exists(Path.Combine(p, cmdParts[0]))))
                {
                    throw new InvalidOperationException($"预启动命令不存在：{cmdParts[0]}");
                }
            }
        }
    }

    private string GenerateOfflineUuid(string username)
    {
        var hash = System.Security.Cryptography.MD5.HashData(System.Text.Encoding.UTF8.GetBytes("OfflinePlayer:" + username));
        hash[6] &= 0x0F;
        hash[6] |= 0x30;
        hash[8] &= 0x3F;
        hash[8] |= 0x80;
        return BitConverter.ToString(hash).Replace("-", "").ToLower();
    }

    public string GetEffectiveGameRootPath()
    {
        return GameRootPath ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            ".minecraft"
        );
    }

    public string GetEffectiveLibrariesPath()
    {
        return LibrariesPath ?? Path.Combine(GetEffectiveGameRootPath(), "libraries");
    }

    public string GetEffectiveVersionJarPath()
    {
        if (!string.IsNullOrWhiteSpace(VersionJarPath)) return VersionJarPath;
        if (string.IsNullOrWhiteSpace(VersionId))
            throw new InvalidOperationException("版本ID未设置，无法生成Jar路径");
        return Path.Combine(GetEffectiveGameRootPath(), "versions", VersionId, $"{VersionId}.jar");
    }

    /// <summary>
    /// 新增：获取需要强制处理的模组列表（用于启动逻辑）
    /// </summary>
    public (HashSet<string> ForceEnable, HashSet<string> ForceDisable) GetModForceLists()
    {
        return (
            new HashSet<string>(ForceEnabledMods, StringComparer.OrdinalIgnoreCase),
            new HashSet<string>(ForceDisabledMods, StringComparer.OrdinalIgnoreCase)
        );
    }
    #endregion
}
