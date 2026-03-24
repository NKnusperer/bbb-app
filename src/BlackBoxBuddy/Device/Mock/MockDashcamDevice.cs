using BlackBoxBuddy.Models;

namespace BlackBoxBuddy.Device.Mock;

public class MockDashcamDevice : IDashcamDevice
{
    private readonly TimeSpan _discoveryDelay;
    private readonly bool _simulateFailure;
    private DeviceInfo? _deviceInfo;

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
    public Task<Dictionary<string, object>> GetSettingsAsync(CancellationToken ct = default)
        => Task.FromResult(new Dictionary<string, object>
        {
            ["wifiBand"] = "5GHz",
            ["drivingMode"] = "Standard",
            ["parkingMode"] = "Standard"
        });

    public Task<bool> ApplySettingsAsync(Dictionary<string, object> settings, CancellationToken ct = default)
        => Task.FromResult(true);

    public Task<bool> ProvisionAsync(Dictionary<string, object> provisioningData, CancellationToken ct = default)
    {
        if (_deviceInfo is not null) _deviceInfo.IsProvisioned = true;
        return Task.FromResult(true);
    }

    public Task<bool> FactoryResetAsync(CancellationToken ct = default)
    {
        if (_deviceInfo is not null) _deviceInfo.IsProvisioned = false;
        return Task.FromResult(true);
    }

    // IDeviceFileSystem
    public Task<IReadOnlyList<string>> ListRecordingsAsync(CancellationToken ct = default)
        => Task.FromResult<IReadOnlyList<string>>(new List<string>
        {
            "/recordings/2026-03-24_08-30-00_front.mp4",
            "/recordings/2026-03-24_08-33-00_front.mp4",
            "/recordings/2026-03-24_08-30-00_rear.mp4"
        });

    public Task<Stream> DownloadFileAsync(string path, CancellationToken ct = default)
        => Task.FromResult<Stream>(new MemoryStream(new byte[1024]));

    public Task<bool> DeleteFileAsync(string path, CancellationToken ct = default)
        => Task.FromResult(true);

    // IDeviceLiveStream
    public Task<Uri?> GetStreamUriAsync(string cameraId, CancellationToken ct = default)
        => Task.FromResult<Uri?>(new Uri($"rtsp://192.168.1.254/live/{cameraId}"));
}
