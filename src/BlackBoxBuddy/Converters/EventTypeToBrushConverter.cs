using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using BlackBoxBuddy.Models;

namespace BlackBoxBuddy.Converters;

public class EventTypeToBrushConverter : IValueConverter
{
    public static readonly EventTypeToBrushConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is EventType eventType)
        {
            return eventType switch
            {
                EventType.None => new SolidColorBrush(Color.Parse("#FF666666")),
                EventType.Radar => new SolidColorBrush(Color.Parse("#FFFFC107")),
                EventType.GShock => new SolidColorBrush(Color.Parse("#FFF44336")),
                EventType.Parking => new SolidColorBrush(Color.Parse("#FFFF9800")),
                _ => new SolidColorBrush(Color.Parse("#FF666666"))
            };
        }

        return new SolidColorBrush(Color.Parse("#FF666666"));
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
