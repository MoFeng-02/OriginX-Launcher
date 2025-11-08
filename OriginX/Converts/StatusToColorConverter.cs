using System;
using System.Globalization;
using Avalonia.Data.Converters;
using MFToolkit.Minecraft.Enums;

namespace OriginX.Converts;

public class StatusToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is DownloadStatus status)
        {
            return status switch
            {
                DownloadStatus.Completed => "#4CAF50",  // 绿色
                DownloadStatus.Failed => "#F44336",      // 红色
                DownloadStatus.Cancelled => "#FF9800",    // 橙色
                DownloadStatus.Downloading => "#2196F3",// 蓝色
                DownloadStatus.Pending => "#9C27B0",    // 紫色
                _ => "#9E9E9E"                           // 灰色
            };
        }
        return "#9E9E9E";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}