namespace BlackBoxBuddy.Device;

public interface IDeviceLiveStream
{
    Task<Uri?> GetStreamUriAsync(string cameraId, CancellationToken ct = default);
}
