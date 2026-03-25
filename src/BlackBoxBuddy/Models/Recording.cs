namespace BlackBoxBuddy.Models;

public record Recording(
    string FileName,
    DateTime DateTime,
    TimeSpan Duration,
    long FileSize,
    double AvgSpeed,
    double PeakGForce,
    double Distance,
    EventType EventType,
    CameraChannel CameraChannel,
    byte[] ThumbnailData);
