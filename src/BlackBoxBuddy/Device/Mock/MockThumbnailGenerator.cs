using BlackBoxBuddy.Models;

namespace BlackBoxBuddy.Device.Mock;

public static class MockThumbnailGenerator
{
    public static byte[] GenerateSolidColor(int width, int height, EventType eventType)
    {
        var (r, g, b) = eventType switch
        {
            EventType.Radar => (255, 193, 7),
            EventType.GShock => (244, 67, 54),
            EventType.Parking => (255, 152, 0),
            _ => (76, 175, 80)
        };
        var pixels = new byte[width * height * 4];
        for (int i = 0; i < pixels.Length; i += 4)
        {
            pixels[i] = (byte)b;        // B
            pixels[i + 1] = (byte)g;    // G
            pixels[i + 2] = (byte)r;    // R
            pixels[i + 3] = 255;        // A
        }
        return pixels;
    }
}
