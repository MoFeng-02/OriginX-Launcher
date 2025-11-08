using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using MFToolkit.Avaloniaui.Helpers;
using MFToolkit.Minecraft.Enums;

namespace OriginX.Features.Resources.VersionManage.Module;

public partial class VersionTypeToIcon : Image
{
    private const string Avares = "avares://OriginX/Assets/Images/Minecraft/";

    private static readonly Dictionary<VersionType, IImage?> Icons = new()
    {
        { VersionType.Release, ImageHelper.LoadFromResource(new Uri(Avares + "Grass_Block_JE2.png")) },
        { VersionType.Snapshot, ImageHelper.LoadFromResource(new Uri(Avares + "Impulse_Command_Block_JE2.png")) },
        { VersionType.OldAlpha, ImageHelper.LoadFromResource(new Uri(Avares + "Jukebox_JE1_BE1.png")) },
        { VersionType.OldBeta, ImageHelper.LoadFromResource(new Uri(Avares + "Lit_Furnace_S_JE1.png")) },
    };
    
    public static readonly StyledProperty<VersionType> VersionTypeProperty =
        AvaloniaProperty.Register<VersionTypeToIcon, VersionType>(
            nameof(VersionType));

    public VersionType VersionType
    {
        get => GetValue(VersionTypeProperty);
        set => SetValue(VersionTypeProperty, value);
    }

    public VersionTypeToIcon()
    {
        InitializeComponent();
        // 初始化时使用当前属性值（可能为默认值）
        Source = Icons.GetValueOrDefault(VersionType);
    }
    static VersionTypeToIcon()
    {
        VersionTypeProperty.Changed.AddClassHandler<VersionTypeToIcon>((sender, e) =>
        {
            var versionType = Enum.Parse<VersionType>(e.NewValue?.ToString()!);
            sender.Source = Icons.GetValueOrDefault(versionType);
        });
    }
}