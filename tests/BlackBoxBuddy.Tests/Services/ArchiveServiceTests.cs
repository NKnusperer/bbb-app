using BlackBoxBuddy.Device;
using BlackBoxBuddy.Models;
using BlackBoxBuddy.Services;
using FluentAssertions;
using NSubstitute;

namespace BlackBoxBuddy.Tests.Services;

public class ArchiveServiceTests
{
    private static Recording MakeRecording(string fileName = "front/2026-01-01_120000.mp4")
        => new Recording(
            FileName: fileName,
            DateTime: new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc),
            Duration: TimeSpan.FromSeconds(30),
            FileSize: 1024,
            AvgSpeed: 60,
            PeakGForce: 1.0,
            Distance: 1.0,
            EventType: EventType.None,
            CameraChannel: CameraChannel.Front,
            ThumbnailData: []);

    private readonly IDashcamDevice _device;
    private readonly ArchiveService _sut;
    private readonly string _testDir;

    public ArchiveServiceTests()
    {
        _device = Substitute.For<IDashcamDevice>();

        // Make DownloadFileAsync return a fresh stream on each call
        var bytes = new byte[] { 1, 2, 3, 4, 5 };
        _device.DownloadFileAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
               .Returns(_ => Task.FromResult<Stream>(new MemoryStream(bytes)));

        _testDir = Path.Combine(Path.GetTempPath(), "BBBTest_" + Guid.NewGuid());
        _sut = new ArchiveService(_device, _testDir);
    }

    [Fact]
    public async Task ArchiveAsync_CallsDownloadFileAsyncWithRecordingFileName()
    {
        var rec = MakeRecording("front/myfile.mp4");
        await _sut.ArchiveAsync(rec);

        await _device.Received(1).DownloadFileAsync("front/myfile.mp4", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ArchiveAsync_WritesFileToArchiveDirectory()
    {
        var rec = MakeRecording("front/myfile.mp4");
        await _sut.ArchiveAsync(rec);

        var expectedPath = Path.Combine(_testDir, "myfile.mp4");
        File.Exists(expectedPath).Should().BeTrue();
    }

    [Fact]
    public async Task ArchiveAsync_ReportsProgressOnCompletion()
    {
        var rec = MakeRecording("front/myfile.mp4");
        var progressValues = new List<double>();
        var progress = new Progress<double>(v => progressValues.Add(v));

        await _sut.ArchiveAsync(rec, progress);

        // Allow progress callbacks to fire
        await Task.Delay(50);
        progressValues.Should().Contain(1.0);
    }

    [Fact]
    public async Task ArchiveTripAsync_DownloadsAllClipsInTrip()
    {
        var rec1 = MakeRecording("front/clip1.mp4");
        var rec2 = MakeRecording("front/clip2.mp4");
        var rec3 = MakeRecording("front/clip3.mp4");
        var trip = new TripGroup([rec1, rec2, rec3]);

        await _sut.ArchiveTripAsync(trip);

        await _device.Received(1).DownloadFileAsync("front/clip1.mp4", Arg.Any<CancellationToken>());
        await _device.Received(1).DownloadFileAsync("front/clip2.mp4", Arg.Any<CancellationToken>());
        await _device.Received(1).DownloadFileAsync("front/clip3.mp4", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ArchiveTripAsync_ReportsProgressAsFractionOfCompletedClips()
    {
        var rec1 = MakeRecording("front/clip1.mp4");
        var rec2 = MakeRecording("front/clip2.mp4");
        var trip = new TripGroup([rec1, rec2]);
        var progressValues = new List<double>();
        var progress = new Progress<double>(v => progressValues.Add(v));

        await _sut.ArchiveTripAsync(trip, progress);
        await Task.Delay(50);

        progressValues.Should().ContainInOrder(0.5, 1.0);
    }

    [Fact]
    public async Task ArchiveAsync_CancellationToken_CancelsDownload()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var rec = MakeRecording("front/myfile.mp4");

        var act = async () => await _sut.ArchiveAsync(rec, ct: cts.Token);
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public void IsArchived_ReturnsTrueWhenFileExists()
    {
        var rec = MakeRecording("front/archived.mp4");
        var expectedPath = Path.Combine(_testDir, "archived.mp4");
        Directory.CreateDirectory(_testDir);
        File.WriteAllBytes(expectedPath, [1, 2, 3]);

        _sut.IsArchived(rec).Should().BeTrue();
    }

    [Fact]
    public void IsArchived_ReturnsFalseWhenFileDoesNotExist()
    {
        var rec = MakeRecording("front/missing.mp4");
        _sut.IsArchived(rec).Should().BeFalse();
    }

    [Fact]
    public void GetArchiveDirectory_ReturnsConfiguredDirectory()
    {
        _sut.GetArchiveDirectory().Should().Be(_testDir);
    }
}
