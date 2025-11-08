using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace OriginX.Converts;

/// <summary>
/// 本地时间转换
/// </summary>
public class LocalTimeConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is DateTimeOffset dateTime)
        {
            return dateTime.ToLocalTime();
        }
        return value;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value;
    }
}