using Bogus;
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
        var faker = new Faker { Random = new Randomizer(42) };
        var baseTime = new DateTime(2026, 3, 20, 8, 0, 0);
        var recordings = new List<Recording>();

        // First 6 recordings: consecutive sequence (timestamps 60 seconds apart) for trip grouping
        for (int i = 0; i < 6; i++)
        {
            var eventType = EventType.None;
            var channel = i % 2 == 0 ? CameraChannel.Front : CameraChannel.Rear;
            var duration = TimeSpan.FromSeconds(faker.Random.Int(60, 180));
            var fileSize = (long)(duration.TotalSeconds * faker.Random.Double(800000, 1200000));
            var avgSpeed = faker.Random.Double(0, 120);
            var peakGForce = faker.Random.Double(0.1, 3.5);
            var distance = duration.TotalSeconds * faker.Random.Double(5, 30) / 3600.0;
            var thumbnail = MockThumbnailGenerator.GenerateSolidColor(160, 90, eventType);
            var dt = baseTime.AddSeconds(i * 20);
            var fileName = $"CLIP_{dt:yyyyMMdd_HHmmss}_{(channel == CameraChannel.Front ? "F" : "R")}.mp4";

            recordings.Add(new Recording(fileName, dt, duration, fileSize, avgSpeed, peakGForce, distance, eventType, channel, thumbnail));
        }

        // Remaining 12: spread randomly across 3 days with mixed event types
        var eventWeights = new[] {
            (EventType.None, 50),
            (EventType.Radar, 20),
            (EventType.GShock, 20),
            (EventType.Parking, 10)
        };

        var channels = new[] { CameraChannel.Front, CameraChannel.Rear };

        for (int i = 0; i < 12; i++)
        {
            var dayOffset = faker.Random.Int(0, 2);
            var hourOffset = faker.Random.Int(0, 23);
            var minOffset = faker.Random.Int(0, 59);
            var dt = baseTime.AddDays(dayOffset).AddHours(hourOffset).AddMinutes(minOffset);

            // Weighted event type selection
            var roll = faker.Random.Int(1, 100);
            var eventType = roll <= 50 ? EventType.None :
                            roll <= 70 ? EventType.Radar :
                            roll <= 90 ? EventType.GShock :
                            EventType.Parking;

            var channel = faker.Random.ArrayElement(channels);
            var duration = TimeSpan.FromSeconds(faker.Random.Int(60, 180));
            var fileSize = (long)(duration.TotalSeconds * faker.Random.Double(800000, 1200000));
            var avgSpeed = faker.Random.Double(0, 120);
            var peakGForce = faker.Random.Double(0.1, 3.5);
            var distance = duration.TotalSeconds * faker.Random.Double(5, 30) / 3600.0;
            var thumbnail = MockThumbnailGenerator.GenerateSolidColor(160, 90, eventType);
            var fileName = $"CLIP_{dt:yyyyMMdd_HHmmss}_{(channel == CameraChannel.Front ? "F" : "R")}.mp4";

            recordings.Add(new Recording(fileName, dt, duration, fileSize, avgSpeed, peakGForce, distance, eventType, channel, thumbnail));
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
