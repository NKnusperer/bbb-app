namespace BlackBoxBuddy.Models.Settings;

public record OverlaySettings(
    bool DateEnabled,
    bool TimeEnabled,
    bool GpsPositionEnabled,
    bool SpeedEnabled,
    SpeedUnit SpeedUnit);
