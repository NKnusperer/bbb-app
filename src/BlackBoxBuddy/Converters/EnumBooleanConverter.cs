using System.Globalization;
using Avalonia.Data.Converters;

namespace BlackBoxBuddy.Converters;

public class EnumBooleanConverter : IValueConverter
{
    public static readonly EnumBooleanConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value?.Equals(parameter) ?? false;

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is true ? parameter : Avalonia.Data.BindingOperations.DoNothing;
}
