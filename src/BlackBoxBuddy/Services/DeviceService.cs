using BlackBoxBuddy.Device;
using BlackBoxBuddy.Models;

namespace BlackBoxBuddy.Services;

public class DeviceService : IDeviceService
{
    private readonly IDashcamDevice _device;
    private static readonly TimeSpan DiscoveryTimeout = TimeSpan.FromSeconds(5);

    public DeviceService(IDashcamDevice device)
    {
        _device = device;
    }

    public ConnectionState ConnectionState { get; private set; } = ConnectionState.Disconnected;
    public DeviceInfo? ConnectedDevice { get; private set; }

    public event EventHandler<ConnectionState>? ConnectionStateChanged;

    public async Task StartDiscoveryAsync(CancellationToken ct = default)
    {
        SetState(ConnectionState.Searching);
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        cts.CancelAfter(DiscoveryTimeout);

        try
        {
            var info = await _device.DiscoverAsync(cts.Token);
            if (info is null)
            {
                SetState(ConnectionState.Disconnected);
            }
            else
            {
                ConnectedDevice = info;
                SetState(info.IsProvisioned
                    ? ConnectionState.Connected
                    : ConnectionState.NeedsProvisioning);
            }
        }
        catch (OperationCanceledException)
        {
            SetState(ConnectionState.Disconnected);
        }
    }

    public async Task<bool> ConnectManuallyAsync(string host, CancellationToken ct = default)
    {
        SetState(ConnectionState.Searching);
        var connected = await _device.ConnectAsync(host, ct);
        if (connected)
        {
            var info = await _device.DiscoverAsync(ct);
            ConnectedDevice = info;
            SetState(info?.IsProvisioned == true
                ? ConnectionState.Connected
                : ConnectionState.NeedsProvisioning);
        }
        else
        {
            SetState(ConnectionState.Disconnected);
        }
        return connected;
    }

    public async Task DisconnectAsync(CancellationToken ct = default)
    {
        await _device.DisconnectAsync(ct);
        ConnectedDevice = null;
        SetState(ConnectionState.Disconnected);
    }

    private void SetState(ConnectionState state)
    {
        ConnectionState = state;
        ConnectionStateChanged?.Invoke(this, state);
    }
}
