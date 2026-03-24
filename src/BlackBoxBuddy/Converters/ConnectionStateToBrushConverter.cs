using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using BlackBoxBuddy.Models;

namespace BlackBoxBuddy.Converters;

public class ConnectionStateToBrushConverter : IValueConverter
{
    public static readonly ConnectionStateToBrushConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is ConnectionState state)
        {
            return state switch
            {
                ConnectionState.Searching => new SolidColorBrush(Color.Parse("#FFFFC107")),
                ConnectionState.Connected => new SolidColorBrush(Color.Parse("#FF4CAF50")),
                ConnectionState.NeedsProvisioning => new SolidColorBrush(Color.Parse("#FFFF9800")),
                ConnectionState.Disconnected => new SolidColorBrush(Color.Parse("#FFF44336")),
                _ => new SolidColorBrush(Color.Parse("#FF666666"))
            };
        }

        return new SolidColorBrush(Color.Parse("#FF666666"));
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
