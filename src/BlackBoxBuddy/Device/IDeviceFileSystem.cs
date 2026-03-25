using BlackBoxBuddy.Models;

namespace BlackBoxBuddy.Device;

public interface IDeviceFileSystem
{
    Task<IReadOnlyList<Recording>> ListRecordingsAsync(CancellationToken ct = default);
    Task<Stream> DownloadFileAsync(string path, CancellationToken ct = default);
    Task<bool> DeleteFileAsync(string path, CancellationToken ct = default);
}
