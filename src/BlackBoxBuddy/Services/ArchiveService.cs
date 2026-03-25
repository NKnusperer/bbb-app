using BlackBoxBuddy.Device;
using BlackBoxBuddy.Models;

namespace BlackBoxBuddy.Services;

public class ArchiveService : IArchiveService
{
    private readonly IDashcamDevice _device;
    private readonly string _archiveDirectory;

    /// <summary>
    /// Production constructor — archives to ~/BlackBoxBuddy/Archives/ (D-15).
    /// </summary>
    public ArchiveService(IDashcamDevice device)
        : this(device, Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            "BlackBoxBuddy",
            "Archives"))
    {
    }

    /// <summary>
    /// Testable constructor with explicit archive directory.
    /// </summary>
    public ArchiveService(IDashcamDevice device, string archiveDirectory)
    {
        _device = device;
        _archiveDirectory = archiveDirectory;
    }

    public string GetArchiveDirectory() => _archiveDirectory;

    public bool IsArchived(Recording recording)
    {
        var destPath = Path.Combine(_archiveDirectory, Path.GetFileName(recording.FileName));
        return File.Exists(destPath);
    }

    public async Task ArchiveAsync(
        Recording recording,
        IProgress<double>? progress = null,
        CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        Directory.CreateDirectory(_archiveDirectory);
        var destPath = Path.Combine(_archiveDirectory, Path.GetFileName(recording.FileName));

        await using var sourceStream = await _device.DownloadFileAsync(recording.FileName, ct);
        await using var destStream = File.Create(destPath);
        await sourceStream.CopyToAsync(destStream, ct);

        progress?.Report(1.0);
    }

    public async Task ArchiveTripAsync(
        TripGroup trip,
        IProgress<double>? progress = null,
        CancellationToken ct = default)
    {
        var clips = trip.Clips;
        int total = clips.Count;
        int completed = 0;

        // Per D-17: create subfolder Trip_{startTime:yyyy-MM-dd_HH-mm-ss}/
        var subfolder = Path.Combine(
            _archiveDirectory,
            $"Trip_{trip.StartTime:yyyy-MM-dd_HH-mm-ss}");
        Directory.CreateDirectory(subfolder);

        foreach (var clip in clips)
        {
            ct.ThrowIfCancellationRequested();

            var destPath = Path.Combine(subfolder, Path.GetFileName(clip.FileName));
            await using var sourceStream = await _device.DownloadFileAsync(clip.FileName, ct);
            await using var destStream = File.Create(destPath);
            await sourceStream.CopyToAsync(destStream, ct);

            completed++;
            progress?.Report((double)completed / total);
        }
    }
}
