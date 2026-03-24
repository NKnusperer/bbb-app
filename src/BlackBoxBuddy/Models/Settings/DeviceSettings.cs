namespace BlackBoxBuddy.Models.Settings;

/// <summary>
/// Composite record aggregating all 7 device settings categories.
/// Record value equality enables dirty-state comparison (pending != loaded).
/// </summary>
public record DeviceSettings(
    WifiSettings Wifi,
    RecordingSettings Recording,
    ChannelSettings Channels,
    CameraSettings Camera,
    SensorSettings Sensors,
    SystemSettings System,
    OverlaySettings Overlays);
