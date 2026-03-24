using BlackBoxBuddy.Models;

namespace BlackBoxBuddy.Device;

public interface IDeviceDiscovery
{
    Task<DeviceInfo?> DiscoverAsync(CancellationToken ct = default);
}
