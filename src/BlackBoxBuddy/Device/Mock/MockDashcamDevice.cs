using BlackBoxBuddy.Models;
using BlackBoxBuddy.Models.Settings;

namespace BlackBoxBuddy.Device.Mock;

public class MockDashcamDevice : IDashcamDevice
{
    private readonly TimeSpan _discoveryDelay;
    private readonly bool _simulateFailure;
    private DeviceInfo? _deviceInfo;

    private static readonly DeviceSettings DefaultSettings = new(
        Wifi: new WifiSettings(WifiBand.FiveGHz, WifiMode.AccessPoint, "DashcamAP", ""),
        Recording: new RecordingSettings(DrivingMode.Standard, ParkingMode.Standard),
        Channels: new ChannelSettings(RecordingChannels.FrontAndRear),
        Camera: new CameraSettings(RearOrientation.Normal),
        Sensors: new SensorSettings(3, 3, 3),
        System: new SystemSettings(true, true, 3),
        Overlays: new OverlaySettings(true, true, false, false, SpeedUnit.KilometersPerHour));

    private DeviceSettings _currentSettings = DefaultSettings;

    private List<string> _recordings = new()
    {
        "/recordings/2026-03-24_08-30-00_front.mp4",
        "/recordings/2026-03-24_08-33-00_front.mp4",
        "/recordings/2026-03-24_08-30-00_rear.mp4"
    };

    public MockDashcamDevice(
        TimeSpan? discoveryDelay = null,
        bool simulateFailure = false,
        bool isProvisioned = true)
    {
        _discoveryDelay = discoveryDelay ?? TimeSpan.FromMilliseconds(100);
        _simulateFailure = simulateFailure;
        _deviceInfo = new DeviceInfo
        {
            DeviceName = "Mock Dashcam Pro",
            FirmwareVersion = "1.0.0-mock",
            IsProvisioned = isProvisioned,
            IpAddress = "192.168.1.254"
        };
    }

    // IDeviceDiscovery
    public async Task<DeviceInfo?> DiscoverAsync(CancellationToken ct = default)
    {
        await Task.Delay(_discoveryDelay, ct);
        return _simulateFailure ? null : _deviceInfo;
    }

    // IDeviceConnection
    public bool IsConnected { get; private set; }

    public async Task<bool> ConnectAsync(string host, CancellationToken ct = default)
    {
        await Task.Delay(TimeSpan.FromMilliseconds(50), ct);
        IsConnected = true;
        return true;
    }

    public async Task DisconnectAsync(CancellationToken ct = default)
    {
        await Task.Delay(TimeSpan.FromMilliseconds(10), ct);
        IsConnected = false;
    }

    // IDeviceCommands
    public Task<DeviceSettings> GetSettingsAsync(CancellationToken ct = default)
        => Task.FromResult(_currentSettings);

    public Task<bool> ApplySettingsAsync(DeviceSettings settings, CancellationToken ct = default)
    {
        _currentSettings = settings;
        return Task.FromResult(true);
    }

    public Task<bool> ProvisionAsync(Dictionary<string, object> provisioningData, CancellationToken ct = default)
    {
        if (_deviceInfo is not null) _deviceInfo.IsProvisioned = true;
        return Task.FromResult(true);
    }

    public Task<bool> FactoryResetAsync(CancellationToken ct = default)
    {
        if (_deviceInfo is not null) _deviceInfo.IsProvisioned = false;
        _currentSettings = DefaultSettings;
        return Task.FromResult(true);
    }

    public Task<bool> WipeSdCardAsync(CancellationToken ct = default)
    {
        _recordings.Clear();
        return Task.FromResult(true);
    }

    // IDeviceFileSystem
    public Task<IReadOnlyList<string>> ListRecordingsAsync(CancellationToken ct = default)
        => Task.FromResult<IReadOnlyList<string>>(_recordings.AsReadOnly());

    public Task<Stream> DownloadFileAsync(string path, CancellationToken ct = default)
        => Task.FromResult<Stream>(new MemoryStream(new byte[1024]));

    public Task<bool> DeleteFileAsync(string path, CancellationToken ct = default)
        => Task.FromResult(true);

    // IDeviceLiveStream
    public Task<Uri?> GetStreamUriAsync(string cameraId, CancellationToken ct = default)
        => Task.FromResult<Uri?>(new Uri($"rtsp://192.168.1.254/live/{cameraId}"));
}
