using System;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using MFToolkit.Minecraft.Enums;

namespace OriginX.Features.Resources.VersionManage.Module;

public partial class SelectItem : Border
{
    public static readonly StyledProperty<VersionType> VersionTypeProperty =
        AvaloniaProperty.Register<SelectItem, VersionType>(
            nameof(VersionType));

    public static readonly StyledProperty<string> TitleProperty = AvaloniaProperty.Register<SelectItem, string>(
        nameof(Title));

    public static readonly StyledProperty<object?> RightContentProperty =
        AvaloniaProperty.Register<SelectItem, object?>(
            nameof(RightContent));

    public static readonly StyledProperty<ICommand?> CommandProperty = AvaloniaProperty.Register<SelectItem, ICommand?>(
        nameof(Command));

    public static readonly StyledProperty<object?> CommandParameterProperty =
        AvaloniaProperty.Register<SelectItem, object?>(
            nameof(CommandParameter));

    public static readonly StyledProperty<DateTimeOffset> ReleaseTimeProperty =
        AvaloniaProperty.Register<SelectItem, DateTimeOffset>(
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

    private PointerPressedEventArgs _e;

    private bool _isPressedOnThisControl;

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            // 记录鼠标是在这个控件上按下的
            _isPressedOnThisControl = true;
            e.Handled = true;
        }
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        if (_isPressedOnThisControl &&
            e.GetCurrentPoint(this).Properties.PointerUpdateKind == PointerUpdateKind.LeftButtonReleased)
        {
            // 检查鼠标是否还在控件范围内
            var position = e.GetPosition(this);
            bool isMouseOverControl = position.X >= 0 && position.Y >= 0 &&
                                      position.X <= Bounds.Width &&
                                      position.Y <= Bounds.Height;

            if (isMouseOverControl)
            {
                // 鼠标在控件上释放，执行操作
                Click?.Invoke(this, _e);
                Command?.Execute(CommandParameter);
            }

            _isPressedOnThisControl = false;
            e.Handled = true;
        }

        base.OnPointerReleased(e);
    }

    public SelectItem()
    {
        InitializeComponent();
    }
}