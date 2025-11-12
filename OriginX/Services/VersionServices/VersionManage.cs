using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using MFToolkit.Abstractions.DependencyInjection;
using MFToolkit.Minecraft.Entities.GameVersion;
using Microsoft.Extensions.Logging;
using OriginX.Extensions.JsonExtensions;
using OriginX.Models.Versions;
using OriginX.Services.VersionServices.Interfaces;

namespace OriginX.Services.VersionServices;

[Singleton<IVersionManage>]
public class VersionManage : IVersionManage
{
    /// <summary>
    /// 进行一次缓存，避免重复检查文件读取IO
    /// </summary>
    private static Dictionary<string, Dictionary<Guid, VersionInfoDetailSimple>>? _indexs;

    /// <summary>
    /// 版本管理列表名称
    /// </summary>
    private const string SaveIndexName = "versions_index.json";

    private static readonly string BaseSaveFolderPath = Path.Combine(AppContext.BaseDirectory, "AppData", "Versions");

    private static readonly string SaveIndexFilePath = Path.Combine(BaseSaveFolderPath, SaveIndexName);


    private static readonly string SaveFileFolderPath = Path.Combine(BaseSaveFolderPath, "files");

    #region DI

    private readonly ILogger<VersionManage> _logger;

    public VersionManage(ILogger<VersionManage> logger)
    {
        _logger = logger;
        // 初始化文件夹
        Directory.CreateDirectory(BaseSaveFolderPath);
        Directory.CreateDirectory(SaveFileFolderPath);
    }

    #endregion

    public async Task<bool> SaveVersionDetailAsync(VersionInfoDetail detail)
    {
        var basePath = detail.BasePath;
        _indexs ??= await LoadVersionDetailSimpleAsync() ?? new Dictionary<string, Dictionary<Guid, VersionInfoDetailSimple>>();
    
        // 确保basePath对应的字典存在
        if (!_indexs.TryGetValue(basePath, out var values))
        {
            values = new Dictionary<Guid, VersionInfoDetailSimple>();
            _indexs[basePath] = values;
        }
        // 如果存在相同ID，直接覆盖；如果不存在，直接添加
        values[detail.Id] = new VersionInfoDetailSimple()
        {
            Id = detail.Id,
            DisplayName = detail.DisplayName,
            VersionType = detail.VersionType,
            ModLoaderType = detail.ModLoaderType,
        };
    
        // 保存详细文件
        var path = Path.Combine(SaveFileFolderPath, detail.Id + ".json");
        await File.WriteAllTextAsync(path,
            JsonSerializer.Serialize(detail, OriginXJsonContext.Default.VersionInfoDetail));
    
        // 保存索引
        await File.WriteAllTextAsync(SaveIndexFilePath,
            JsonSerializer.Serialize(_indexs, OriginXJsonContext.Default.DictionaryStringDictionaryGuidVersionInfoDetailSimple));
    
        return true;
    }

    public async Task<VersionInfoDetail?> GetVersionDetailAsync(Guid id)
    {
        _indexs ??= await LoadVersionDetailSimpleAsync();
        if (_indexs == null) return null;
    
        // 修复：在所有basePath中查找对应的id
        foreach (var values in _indexs.Values)
        {
            if (!values.TryGetValue(id, out _)) continue;
            var path = Path.Combine(SaveFileFolderPath, $"{id}.json");
            
            // 修复：正确的文件存在性检查
            if (!File.Exists(path))
            {
                // 文件被删除但索引还在，清理索引
                values.Remove(id);
                await SaveIndexAsync(); // 需要添加保存索引的方法
                return null;
            }
            
            try
            {
                var json = await File.ReadAllTextAsync(path);
                return JsonSerializer.Deserialize<VersionInfoDetail>(json, OriginXJsonContext.Default.VersionInfoDetail);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "读取版本详情文件时出错");
                return null;
            }
        }
    
        return null;
    }

    public async Task<Dictionary<string, Dictionary<Guid, VersionInfoDetailSimple>>?> LoadVersionDetailSimpleAsync()
    {
        var fileExists = File.Exists(SaveIndexFilePath);
        // 当文件不存在直接返回null
        if (!fileExists) return null;
        // 当缓存的列表不为空的时候返回它
        if (_indexs != null) return _indexs;

        // 存在，则读取
        await using var fileStream = new FileStream(SaveIndexFilePath, FileMode.Open);
        try
        {
            _indexs = await JsonSerializer.DeserializeAsync(fileStream,
                OriginXJsonContext.Default.DictionaryStringDictionaryGuidVersionInfoDetailSimple);
            return _indexs;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            _logger.LogError($"读取一次版本管理索引的时候出错，错误信息{e.Message}");
            throw;
        }
    }
    // 添加辅助方法保存索引
    private async Task SaveIndexAsync()
    {
        if (_indexs != null)
        {
            await File.WriteAllTextAsync(SaveIndexFilePath,
                JsonSerializer.Serialize(_indexs, OriginXJsonContext.Default.DictionaryStringDictionaryGuidVersionInfoDetailSimple));
        }
    }
}