namespace BlackBoxBuddy.Device;

public interface IDeviceCommands
{
    Task<Dictionary<string, object>> GetSettingsAsync(CancellationToken ct = default);
    Task<bool> ApplySettingsAsync(Dictionary<string, object> settings, CancellationToken ct = default);
    Task<bool> ProvisionAsync(Dictionary<string, object> provisioningData, CancellationToken ct = default);
    Task<bool> FactoryResetAsync(CancellationToken ct = default);
}
