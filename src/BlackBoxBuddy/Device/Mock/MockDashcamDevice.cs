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

    private List<Recording> _recordings;

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
        _recordings = GenerateMockRecordings();
    }

    private static List<Recording> GenerateMockRecordings()
    {
        // Temporary minimal recordings — will be replaced with Bogus in Task 2
        var baseTime = new DateTime(2026, 3, 20, 8, 0, 0);
        var thumbnailData = MockThumbnailGenerator.GenerateSolidColor(160, 90, EventType.None);
        var recordings = new List<Recording>();
        for (int i = 0; i < 3; i++)
        {
            recordings.Add(new Recording(
                $"CLIP_20260320_0{8 + i * 3:D2}0000_F.mp4",
                baseTime.AddMinutes(i * 3),
                TimeSpan.FromMinutes(2),
                100000L,
                60.0,
                1.0,
                1.0,
                EventType.None,
                CameraChannel.Front,
                thumbnailData));
        }
        return recordings;
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
    public Task<IReadOnlyList<Recording>> ListRecordingsAsync(CancellationToken ct = default)
        => Task.FromResult<IReadOnlyList<Recording>>(_recordings.AsReadOnly());

    public Task<Stream> DownloadFileAsync(string path, CancellationToken ct = default)
    {
        var assembly = typeof(MockDashcamDevice).Assembly;
        var stream = assembly.GetManifestResourceStream("BlackBoxBuddy.Assets.sample.mp4");
        if (stream is null)
            throw new FileNotFoundException("Embedded resource sample.mp4 not found");
        return Task.FromResult<Stream>(stream);
    }

    public Task<bool> DeleteFileAsync(string path, CancellationToken ct = default)
        => Task.FromResult(true);

    // IDeviceLiveStream
    public Task<Uri?> GetStreamUriAsync(string cameraId, CancellationToken ct = default)
        => Task.FromResult<Uri?>(new Uri($"rtsp://192.168.1.254/live/{cameraId}"));
}
