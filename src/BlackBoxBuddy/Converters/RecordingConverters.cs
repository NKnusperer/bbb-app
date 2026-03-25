using System.Globalization;
using Avalonia.Data.Converters;
using BlackBoxBuddy.Models;

namespace BlackBoxBuddy.Converters;

/// <summary>
/// Converts a long byte count to a human-readable MB string, e.g. "9.8 MB".
/// </summary>
public class FileSizeConverter : IValueConverter
{
    public static readonly FileSizeConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is long bytes)
            return (bytes / 1_048_576.0).ToString("F1", culture) + " MB";
        return null;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

/// <summary>
/// Converts EventType to a display string: "Radar", "G-Shock", "Parking", or empty for None.
/// </summary>
public class EventTypeToStringConverter : IValueConverter
{
    public static readonly EventTypeToStringConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is EventType eventType)
        {
            return eventType switch
            {
                EventType.Radar => "Radar",
                EventType.GShock => "G-Shock",
                EventType.Parking => "Parking",
                _ => string.Empty
            };
        }
        return string.Empty;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

/// <summary>
/// Returns true (visible) for non-None EventType values (for badge visibility).
/// </summary>
public class EventTypeToVisibilityConverter : IValueConverter
{
    public static readonly EventTypeToVisibilityConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is EventType eventType)
            return eventType != EventType.None;
        return false;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

/// <summary>
/// Returns true for null EventType? (i.e., "All" filter is selected).
/// Used for filter chip active state.
/// </summary>
public class NullFilterActiveBrushConverter : IValueConverter
{
    public static readonly NullFilterActiveBrushConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        // value is the SelectedFilter (EventType?), parameter is the EventType value to check
        // If both null -> active. If value matches parameter -> active.
        if (parameter is null)
            return value is null ? "#2196F3" : "#1EFFFFFF";

        if (parameter is EventType paramType && value is EventType selectedType)
            return selectedType == paramType ? "#2196F3" : "#1EFFFFFF";

        return "#1EFFFFFF";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
