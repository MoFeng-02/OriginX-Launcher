using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using MFToolkit.Minecraft.Entities.GameVersion;
using MFToolkit.Minecraft.JsonExtensions;
using OriginX.Models.Versions;

namespace OriginX.Extensions.JsonExtensions;

[JsonSourceGenerationOptions(
    WriteIndented = false, // 关闭缩进，减小体积
    PropertyNameCaseInsensitive = true,
    IncludeFields = true)]
[JsonSerializable(typeof(Dictionary<string, List<VersionInfoDetail>>))]
[JsonSerializable(typeof(VersionInfoDetailSimple))]
[JsonSerializable(typeof(Dictionary<string, Dictionary<Guid, VersionInfoDetailSimple>>))]
public partial class OriginXJsonContext : MinecraftJsonSerializerContext
{
}