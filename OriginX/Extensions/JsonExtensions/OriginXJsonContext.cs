using System.Collections.Generic;
using System.Text.Json.Serialization;
using OriginX.Models.Versions;
using OriginX.Options.MinecraftOptions;

namespace OriginX.Extensions.JsonExtensions;

[JsonSourceGenerationOptions(
    WriteIndented = false, // 关闭缩进，减小体积
    PropertyNameCaseInsensitive = true,
    IncludeFields = true)]
#region 选项
[JsonSerializable(typeof(StorageOptions))]
[JsonSerializable(typeof(LaunchOptions))]
#endregion
#region 模型
[JsonSerializable(typeof(VersionGroup))]
[JsonSerializable(typeof(List<VersionGroup>))]
[JsonSerializable(typeof(VersionInfo))]
[JsonSerializable(typeof(List<VersionInfo>))]
#endregion
public partial class OriginXJsonContext : JsonSerializerContext
{
}
