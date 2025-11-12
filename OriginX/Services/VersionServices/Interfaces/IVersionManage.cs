using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using MFToolkit.Minecraft.Entities.GameVersion;
using OriginX.Models.Versions;

namespace OriginX.Services.VersionServices.Interfaces;

/// <summary>
/// 提供版本管理等
/// </summary>
public interface IVersionManage
{
    /// <summary>
    /// 保存版本相关信息
    /// </summary>
    /// <param name="detail"></param>
    /// <returns></returns>
    Task<bool> SaveVersionDetailAsync(VersionInfoDetail detail);

    /// <summary>
    /// 根据ID查找
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    /// <exception cref="FileNotFoundException">文件不存在则无法读取的情况下</exception>
    Task<VersionInfoDetail?> GetVersionDetailAsync(Guid id);
    
    /// <summary>
    /// 获取版本相关的简略信息，也和资源索引表差不多的，如果文件不存在则会直接返回null
    /// </summary>
    /// <remarks>
    /// Key: 基础路径，用于分类
    /// Value: 版本列表
    /// </remarks>
    /// <returns></returns>
    Task<Dictionary<string, Dictionary<Guid, VersionInfoDetailSimple>>?> LoadVersionDetailSimpleAsync();
}