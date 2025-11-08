using System;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using MFToolkit.Minecraft.Enums;

namespace OriginX.Features.Resources.VersionManage.Module;


public partial class SelectItem : Border
{
    public static readonly StyledProperty<VersionType> VersionTypeProperty = AvaloniaProperty.Register<SelectItem, VersionType>(
        nameof(VersionType));

    public static readonly StyledProperty<string> TitleProperty = AvaloniaProperty.Register<SelectItem, string>(
        nameof(Title));

    public static readonly StyledProperty<object?> RightContentProperty = AvaloniaProperty.Register<SelectItem, object?>(
        nameof(RightContent));

    public static readonly StyledProperty<ICommand?> CommandProperty = AvaloniaProperty.Register<SelectItem, ICommand?>(
        nameof(Command));

    public static readonly StyledProperty<object?> CommandParameterProperty = AvaloniaProperty.Register<SelectItem, object?>(
        nameof(CommandParameter));

    public static readonly StyledProperty<DateTimeOffset> ReleaseTimeProperty = AvaloniaProperty.Register<SelectItem, DateTimeOffset>(
        nameof(ReleaseTime));

    public DateTimeOffset ReleaseTime
    {
        get => GetValue(ReleaseTimeProperty);
        set => SetValue(ReleaseTimeProperty, value);
    }
    public object? CommandParameter
    {
        get => GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }
    
    public ICommand? Command
    {
        get => GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }
    
    public object? RightContent
    {
        get => GetValue(RightContentProperty);
        set => SetValue(RightContentProperty, value);
    }
    
    public string Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }
    
    public VersionType VersionType
    {
        get => GetValue(VersionTypeProperty);
        set => SetValue(VersionTypeProperty, value);
    }
    
    #region Events

    // 事件访问器
    public event EventHandler<RoutedEventArgs>? Click;

    #endregion

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        Click?.Invoke(this, e);
        Command?.Execute(CommandParameter);
    }

    public SelectItem()
    {
        InitializeComponent();
    }
}