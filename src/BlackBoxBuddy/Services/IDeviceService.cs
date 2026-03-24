using BlackBoxBuddy.Models;

namespace BlackBoxBuddy.Services;

public interface IDeviceService
{
    Models.ConnectionState ConnectionState { get; }
    DeviceInfo? ConnectedDevice { get; }
    Task StartDiscoveryAsync(CancellationToken ct = default);
    Task<bool> ConnectManuallyAsync(string host, CancellationToken ct = default);
    Task DisconnectAsync(CancellationToken ct = default);
    Task<bool> ProvisionAsync(Dictionary<string, object> provisioningData, CancellationToken ct = default);
    event EventHandler<Models.ConnectionState>? ConnectionStateChanged;
}
