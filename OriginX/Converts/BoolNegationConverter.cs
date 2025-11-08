using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace OriginX.Converts;

/// <summary>
/// bool 取反
/// </summary>
public class BoolNegationConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool boolValue) return !boolValue;
        return value;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool boolValue) return !boolValue;
        return value;
    }
}