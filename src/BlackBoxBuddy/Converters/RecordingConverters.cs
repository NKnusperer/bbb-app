using System.Collections.ObjectModel;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
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

    private static readonly SolidColorBrush ActiveBrush = new(Color.Parse("#FF2196F3"));
    private static readonly SolidColorBrush InactiveBrush = new(Color.Parse("#1EFFFFFF"));

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        // value is the SelectedFilter (EventType?), parameter is the EventType value to check
        // If both null -> active ("All" chip). If value matches parameter -> active.
        if (parameter is null)
            return value is null ? ActiveBrush : InactiveBrush;

        if (parameter is EventType paramType && value is EventType selectedType)
            return selectedType == paramType ? ActiveBrush : InactiveBrush;

        return InactiveBrush;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

/// <summary>
/// Multi-value converter for the "Archived" badge.
/// values[0] = FileName (string)
/// values[1] = ArchivedFileNames (HashSet&lt;string&gt;)
/// Returns true when the filename is in the archived set.
/// </summary>
public class ArchivedMultiConverter : IMultiValueConverter
{
    public static readonly ArchivedMultiConverter Instance = new();

    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count < 2) return false;
        var fileName = values[0] as string;
        var archivedSet = values[1] as HashSet<string>;
        if (fileName is null || archivedSet is null) return false;
        return archivedSet.Contains(fileName);
    }
}

/// <summary>
/// Multi-value converter for the selection checkbox IsChecked state.
/// values[0] = Recording (the item)
/// values[1] = SelectedRecordings (ObservableCollection&lt;Recording&gt;)
/// Returns true when the recording is in the selection collection.
/// </summary>
public class RecordingIsSelectedConverter : IMultiValueConverter
{
    public static readonly RecordingIsSelectedConverter Instance = new();

    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count < 2) return false;
        var recording = values[0] as Recording;
        var selected = values[1] as System.Collections.ObjectModel.ObservableCollection<Recording>;
        if (recording is null || selected is null) return false;
        return selected.Contains(recording);
    }
}
