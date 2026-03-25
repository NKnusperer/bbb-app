using System.Globalization;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace BlackBoxBuddy.Converters;

public class BytesToBitmapConverter : IValueConverter
{
    public static readonly BytesToBitmapConverter Instance = new();

    // Default dimensions for mock thumbnails — overridable via parameter "WxH"
    private const int DefaultWidth = 160;
    private const int DefaultHeight = 90;

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not byte[] pixels || pixels.Length == 0) return null;

        int width = DefaultWidth, height = DefaultHeight;
        if (parameter is string dims && dims.Contains('x'))
        {
            var parts = dims.Split('x');
            if (parts.Length == 2
                && int.TryParse(parts[0], out var w)
                && int.TryParse(parts[1], out var h))
            {
                width = w;
                height = h;
            }
        }

        var bmp = new WriteableBitmap(
            new PixelSize(width, height),
            new Vector(96, 96),
            PixelFormat.Bgra8888,
            AlphaFormat.Premul);

        using var fb = bmp.Lock();
        Marshal.Copy(pixels, 0, fb.Address, Math.Min(pixels.Length, width * height * 4));
        return bmp;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
