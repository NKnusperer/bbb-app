using BlackBoxBuddy.Models.Settings;

namespace BlackBoxBuddy.Device;

public interface IDeviceCommands
{
    Task<DeviceSettings> GetSettingsAsync(CancellationToken ct = default);
    Task<bool> ApplySettingsAsync(DeviceSettings settings, CancellationToken ct = default);
    Task<bool> ProvisionAsync(Dictionary<string, object> provisioningData, CancellationToken ct = default);
    Task<bool> FactoryResetAsync(CancellationToken ct = default);
    Task<bool> WipeSdCardAsync(CancellationToken ct = default);
}
