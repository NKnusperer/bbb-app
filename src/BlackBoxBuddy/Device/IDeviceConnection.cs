namespace BlackBoxBuddy.Device;

public interface IDeviceConnection
{
    Task<bool> ConnectAsync(string host, CancellationToken ct = default);
    Task DisconnectAsync(CancellationToken ct = default);
    bool IsConnected { get; }
}
