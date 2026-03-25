using BlackBoxBuddy.Models;
using BlackBoxBuddy.Navigation;
using BlackBoxBuddy.Services;
using BlackBoxBuddy.ViewModels;
using FluentAssertions;
using NSubstitute;

namespace BlackBoxBuddy.Tests.ViewModels;

public class RecordingDetailViewModelTests
{
    private readonly IMediaPlayerService _mediaPlayerService;
    private readonly IArchiveService _archiveService;
    private readonly INavigationService _navigationService;
    private readonly Recording _recording;
    private readonly TripGroup _trip;

    public RecordingDetailViewModelTests()
    {
        _mediaPlayerService = Substitute.For<IMediaPlayerService>();
        _archiveService = Substitute.For<IArchiveService>();
        _navigationService = Substitute.For<INavigationService>();

        _mediaPlayerService.CreatePlayer().Returns(new object());

        _recording = MakeRecording(CameraChannel.Front);

        var clip1 = MakeRecording(CameraChannel.Front, TimeSpan.FromSeconds(30), 60.0, 1.2, 0.5);
        var clip2 = MakeRecording(CameraChannel.Front, TimeSpan.FromSeconds(45), 80.0, 1.5, 0.8);
        _trip = new TripGroup(new[] { clip1, clip2 });
    }

    private static Recording MakeRecording(
        CameraChannel channel = CameraChannel.Front,
        TimeSpan? duration = null,
        double avgSpeed = 60.0,
        double peakGForce = 1.2,
        double distance = 0.5) => new(
            FileName: "clip.mp4",
            DateTime: new DateTime(2025, 6, 15, 10, 30, 0),
            Duration: duration ?? TimeSpan.FromSeconds(30),
            FileSize: 10_485_760,
            AvgSpeed: avgSpeed,
            PeakGForce: peakGForce,
            Distance: distance,
            EventType: EventType.None,
            CameraChannel: channel,
            ThumbnailData: new byte[160 * 90 * 4]);

    private RecordingDetailViewModel CreateSingle() =>
        new(_recording, _mediaPlayerService, _archiveService, _navigationService);

    private RecordingDetailViewModel CreateTrip() =>
        new(_trip, _mediaPlayerService, _archiveService, _navigationService);

    // ── Constructor: Single Recording ────────────────────────────────────────

    [Fact]
    public void Constructor_SingleRecording_SetsIsTripFalse()
    {
        var vm = CreateSingle();
        vm.IsTrip.Should().BeFalse();
    }

    [Fact]
    public void Constructor_SingleRecording_SetsCurrentRecording()
    {
        var vm = CreateSingle();
        vm.CurrentRecording.Should().Be(_recording);
    }

    [Fact]
    public void Constructor_SingleRecording_PageTitleIsRecordingDetails()
    {
        var vm = CreateSingle();
        vm.PageTitle.Should().Be("Recording Details");
    }

    [Fact]
    public void Constructor_SingleRecording_MetadataFormattedCorrectly()
    {
        var vm = CreateSingle();
        vm.DisplayDateTime.Should().NotBeNullOrEmpty();
        vm.DisplayDuration.Should().NotBeNullOrEmpty();
        vm.DisplayFileSize.Should().Contain("MB");
        vm.DisplayAvgSpeed.Should().Contain("km/h");
        vm.DisplayPeakGForce.Should().Contain("g");
        vm.DisplayDistance.Should().Contain("km");
    }

    // ── Constructor: Trip ─────────────────────────────────────────────────────

    [Fact]
    public void Constructor_TripGroup_SetsIsTripTrue()
    {
        var vm = CreateTrip();
        vm.IsTrip.Should().BeTrue();
    }

    [Fact]
    public void Constructor_TripGroup_PageTitleIsTripDetails()
    {
        var vm = CreateTrip();
        vm.PageTitle.Should().Be("Trip Details");
    }

    [Fact]
    public void Constructor_TripGroup_CurrentClipsIsNotNull()
    {
        var vm = CreateTrip();
        vm.CurrentClips.Should().NotBeNull();
        vm.CurrentClips!.Count.Should().Be(2);
    }

    [Fact]
    public void Constructor_TripGroup_ClipCountMatchesTrip()
    {
        var vm = CreateTrip();
        vm.ClipCount.Should().Be(2);
    }

    // ── IsDualCamera ─────────────────────────────────────────────────────────

    [Fact]
    public void IsDualCamera_TrueWhenCameraChannelIsBoth()
    {
        var recording = MakeRecording(CameraChannel.Both);
        var vm = new RecordingDetailViewModel(recording, _mediaPlayerService, _archiveService, _navigationService);
        vm.IsDualCamera.Should().BeTrue();
    }

    [Fact]
    public void IsDualCamera_FalseWhenCameraChannelIsFront()
    {
        var vm = CreateSingle(); // CameraChannel.Front
        vm.IsDualCamera.Should().BeFalse();
    }

    // ── PlayPauseCommand ──────────────────────────────────────────────────────

    [Fact]
    public void PlayPauseCommand_TogglesIsPlayingFromFalseToTrue()
    {
        var vm = CreateSingle();
        vm.IsPlaying.Should().BeFalse();
        vm.PlayPauseCommand.Execute(null);
        vm.IsPlaying.Should().BeTrue();
    }

    [Fact]
    public void PlayPauseCommand_TogglesIsPlayingFromTrueToFalse()
    {
        var vm = CreateSingle();
        vm.PlayPauseCommand.Execute(null); // now playing
        vm.PlayPauseCommand.Execute(null); // now paused
        vm.IsPlaying.Should().BeFalse();
    }

    // ── SetRateCommand ────────────────────────────────────────────────────────

    [Fact]
    public void SetRateCommand_UpdatesPlaybackRate()
    {
        var vm = CreateSingle();
        vm.SetRateCommand.Execute(2.0f);
        vm.PlaybackRate.Should().Be(2.0f);
    }

    [Fact]
    public void SetRateCommand_DefaultPlaybackRateIsOne()
    {
        var vm = CreateSingle();
        vm.PlaybackRate.Should().Be(1.0f);
    }

    // ── NextFrame / PreviousFrame ─────────────────────────────────────────────

    [Fact]
    public void NextFrameCommand_CallsMediaPlayerServiceNextFrame()
    {
        var playerHandle = new object();
        _mediaPlayerService.CreatePlayer().Returns(playerHandle);

        var vm = CreateSingle();
        vm.NextFrameCommand.Execute(null);

        _mediaPlayerService.Received().NextFrame(Arg.Any<object>());
    }

    [Fact]
    public void PreviousFrameCommand_CallsMediaPlayerServicePreviousFrame()
    {
        var playerHandle = new object();
        _mediaPlayerService.CreatePlayer().Returns(playerHandle);

        var vm = CreateSingle();
        vm.PreviousFrameCommand.Execute(null);

        _mediaPlayerService.Received().PreviousFrame(Arg.Any<object>());
    }

    // ── SeekToCommand ─────────────────────────────────────────────────────────

    [Fact]
    public void SeekToCommand_CallsMediaPlayerServiceSeek()
    {
        var vm = CreateSingle();
        vm.SeekToCommand.Execute(0.5f);

        _mediaPlayerService.Received().Seek(Arg.Any<object>(), 0.5f);
    }

    // ── ArchiveCommand ────────────────────────────────────────────────────────

    [Fact]
    public async Task ArchiveRecordingCommand_SingleRecording_CallsArchiveAsync()
    {
        _archiveService
            .ArchiveAsync(Arg.Any<Recording>(), Arg.Any<IProgress<double>>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        var vm = CreateSingle();
        await vm.ArchiveRecordingCommand.ExecuteAsync(null);

        await _archiveService.Received().ArchiveAsync(
            _recording, Arg.Any<IProgress<double>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ArchiveRecordingCommand_TripGroup_CallsArchiveTripAsync()
    {
        _archiveService
            .ArchiveTripAsync(Arg.Any<TripGroup>(), Arg.Any<IProgress<double>>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        var vm = CreateTrip();
        await vm.ArchiveRecordingCommand.ExecuteAsync(null);

        await _archiveService.Received().ArchiveTripAsync(
            _trip, Arg.Any<IProgress<double>>(), Arg.Any<CancellationToken>());
    }

    // ── CancelArchiveCommand ──────────────────────────────────────────────────

    [Fact]
    public async Task CancelArchiveCommand_CancelsOngoingArchive()
    {
        var tcs = new TaskCompletionSource();
        _archiveService
            .ArchiveAsync(Arg.Any<Recording>(), Arg.Any<IProgress<double>>(), Arg.Any<CancellationToken>())
            .Returns(async (callInfo) =>
            {
                var ct = callInfo.Arg<CancellationToken>();
                try
                {
                    await Task.Delay(5000, ct);
                }
                catch (OperationCanceledException)
                {
                    tcs.SetResult();
                    throw;
                }
            });

        var vm = CreateSingle();
        var archiveTask = vm.ArchiveRecordingCommand.ExecuteAsync(null);

        // Give the archive a moment to start
        await Task.Delay(50);

        vm.CancelArchiveCommand.Execute(null);
        await archiveTask;

        // If cancel worked, tcs should be completed
        tcs.Task.IsCompleted.Should().BeTrue();
        vm.IsArchiving.Should().BeFalse();
    }

    // ── Dispose ───────────────────────────────────────────────────────────────

    [Fact]
    public void Dispose_CallsDisposePlayerOnMediaPlayerService()
    {
        var vm = CreateSingle();
        vm.Dispose();

        _mediaPlayerService.Received().DisposePlayer(Arg.Any<object>());
    }
}
