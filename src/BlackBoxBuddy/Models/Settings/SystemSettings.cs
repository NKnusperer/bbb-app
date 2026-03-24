namespace BlackBoxBuddy.Models.Settings;

/// <summary>
/// System-level device settings.
/// SpeakerVolume: 0 = disabled, 1-5 = active volume levels.
/// </summary>
public record SystemSettings(bool GpsEnabled, bool MicrophoneEnabled, int SpeakerVolume);
