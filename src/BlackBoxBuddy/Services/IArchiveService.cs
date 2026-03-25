using BlackBoxBuddy.Models;

namespace BlackBoxBuddy.Services;

public interface IArchiveService
{
    Task ArchiveAsync(Recording recording, IProgress<double>? progress = null, CancellationToken ct = default);
    Task ArchiveTripAsync(TripGroup trip, IProgress<double>? progress = null, CancellationToken ct = default);
    string GetArchiveDirectory();
    bool IsArchived(Recording recording);
}
