using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using MFToolkit.Minecraft.Enums;

namespace OriginX.Models.Versions;

/// <summary>
/// 简洁的详情项
/// </summary>
public class VersionInfoDetailSimple
{
    /// <summary>
    /// Id版本
    /// </summary>
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    /// <summary>
    /// 显示名称
    /// </summary>
    [JsonPropertyName("displayName")]
    public required string DisplayName { get; set; }
    
    [JsonPropertyName("modLoaderType")]
    public ModLoaderType ModLoaderType { get; set; }

    [JsonPropertyName("versionType")]
    public VersionType VersionType { get; set; }
}