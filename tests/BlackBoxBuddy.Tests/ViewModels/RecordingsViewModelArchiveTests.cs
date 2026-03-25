using BlackBoxBuddy.Device;
using BlackBoxBuddy.Models;
using BlackBoxBuddy.Navigation;
using BlackBoxBuddy.Services;
using BlackBoxBuddy.ViewModels;
using FluentAssertions;
using NSubstitute;

namespace BlackBoxBuddy.Tests.ViewModels;

public class RecordingsViewModelArchiveTests
{
    private readonly IDashcamDevice _device;
    private readonly IDeviceService _deviceService;
    private readonly ITripGroupingService _tripGroupingService;
    private readonly IArchiveService _archiveService;
    private readonly INavigationService _navigationService;
    private readonly IMediaPlayerService _mediaPlayerService;
    private readonly RecordingsViewModel _viewModel;

    public RecordingsViewModelArchiveTests()
    {
        _device = Substitute.For<IDashcamDevice>();
        _deviceService = Substitute.For<IDeviceService>();
        _tripGroupingService = Substitute.For<ITripGroupingService>();
        _archiveService = Substitute.For<IArchiveService>();
        _navigationService = Substitute.For<INavigationService>();
        _mediaPlayerService = Substitute.For<IMediaPlayerService>();

        _device.ListRecordingsAsync(Arg.Any<CancellationToken>())
            .Returns(Array.Empty<Recording>());
        _tripGroupingService.Group(Arg.Any<IReadOnlyList<Recording>>())
            .Returns(Array.Empty<object>());
        _deviceService.ConnectionState.Returns(ConnectionState.Disconnected);
        _mediaPlayerService.CreatePlayer().Returns(new object());

        _viewModel = new RecordingsViewModel(
            _device, _deviceService, _tripGroupingService, _archiveService, _navigationService, _mediaPlayerService);
    }

    private static Recording MakeRecording(string fileName = "clip.mp4") => new(
        FileName: fileName,
        DateTime: DateTime.Now,
        Duration: TimeSpan.FromSeconds(30),
        FileSize: 10_485_760,
        AvgSpeed: 60.0,
        PeakGForce: 1.2,
        Distance: 0.5,
        EventType: EventType.None,
        CameraChannel: CameraChannel.Front,
        ThumbnailData: new byte[160 * 90 * 4]);

    // ── ToggleMultiSelect Tests ───────────────────────────────────────────────

    [Fact]
    public void ToggleMultiSelectCommand_SetsIsMultiSelectModeToTrue()
    {
        _viewModel.IsMultiSelectMode.Should().BeFalse();

        _viewModel.ToggleMultiSelectCommand.Execute(null);

        _viewModel.IsMultiSelectMode.Should().BeTrue();
    }

    [Fact]
    public void ToggleMultiSelectCommand_TogglesBackToFalse()
    {
        _viewModel.ToggleMultiSelectCommand.Execute(null);
        _viewModel.ToggleMultiSelectCommand.Execute(null);

        _viewModel.IsMultiSelectMode.Should().BeFalse();
    }

    [Fact]
    public void ToggleMultiSelectCommand_ExitingClearsSelectedRecordings()
    {
        var recording = MakeRecording();
        _viewModel.ToggleMultiSelectCommand.Execute(null);
        _viewModel.ToggleRecordingSelectionCommand.Execute(recording);
        _viewModel.SelectedRecordings.Should().HaveCount(1);

        // Exit multi-select
        _viewModel.ToggleMultiSelectCommand.Execute(null);

        _viewModel.SelectedRecordings.Should().BeEmpty();
    }

    // ── ToggleRecordingSelection Tests ────────────────────────────────────────

    [Fact]
    public void ToggleRecordingSelectionCommand_AddsRecordingToSelectedRecordings()
    {
        var recording = MakeRecording();
        _viewModel.ToggleMultiSelectCommand.Execute(null);

        _viewModel.ToggleRecordingSelectionCommand.Execute(recording);

        _viewModel.SelectedRecordings.Should().Contain(recording);
    }

    [Fact]
    public void ToggleRecordingSelectionCommand_RemovesRecordingIfAlreadySelected()
    {
        var recording = MakeRecording();
        _viewModel.ToggleMultiSelectCommand.Execute(null);
        _viewModel.ToggleRecordingSelectionCommand.Execute(recording);

        _viewModel.ToggleRecordingSelectionCommand.Execute(recording);

        _viewModel.SelectedRecordings.Should().BeEmpty();
    }

    [Fact]
    public void SelectedCount_ReflectsNumberOfSelectedRecordings()
    {
        var r1 = MakeRecording("clip1.mp4");
        var r2 = MakeRecording("clip2.mp4");
        _viewModel.ToggleMultiSelectCommand.Execute(null);

        _viewModel.ToggleRecordingSelectionCommand.Execute(r1);
        _viewModel.SelectedCount.Should().Be(1);

        _viewModel.ToggleRecordingSelectionCommand.Execute(r2);
        _viewModel.SelectedCount.Should().Be(2);
    }

    // ── SelectAll Tests ───────────────────────────────────────────────────────

    [Fact]
    public async Task SelectAllCommand_AddsAllVisibleRecordingsToSelectedRecordings()
    {
        var r1 = MakeRecording("clip1.mp4");
        var r2 = MakeRecording("clip2.mp4");
        var recordings = new[] { r1, r2 };
        _device.ListRecordingsAsync(Arg.Any<CancellationToken>()).Returns(recordings);
        _tripGroupingService.Group(Arg.Any<IReadOnlyList<Recording>>()).Returns(new object[] { r1, r2 });

        await _viewModel.LoadRecordingsCommand.ExecuteAsync(null);
        _viewModel.ToggleMultiSelectCommand.Execute(null);

        _viewModel.SelectAllCommand.Execute(null);

        _viewModel.SelectedRecordings.Should().HaveCount(2);
    }

    // ── ArchiveSelected Tests ─────────────────────────────────────────────────

    [Fact]
    public async Task ArchiveSelectedCommand_CallsArchiveAsyncForEachSelectedRecording()
    {
        var r1 = MakeRecording("clip1.mp4");
        var r2 = MakeRecording("clip2.mp4");
        _archiveService.ArchiveAsync(Arg.Any<Recording>(), Arg.Any<IProgress<double>?>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        _viewModel.ToggleMultiSelectCommand.Execute(null);
        _viewModel.ToggleRecordingSelectionCommand.Execute(r1);
        _viewModel.ToggleRecordingSelectionCommand.Execute(r2);

        await _viewModel.ArchiveSelectedCommand.ExecuteAsync(null);

        await _archiveService.Received(1).ArchiveAsync(r1, Arg.Any<IProgress<double>?>(), Arg.Any<CancellationToken>());
        await _archiveService.Received(1).ArchiveAsync(r2, Arg.Any<IProgress<double>?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ArchiveSelectedCommand_ReportsProgressAsCompletedOverTotal()
    {
        var r1 = MakeRecording("clip1.mp4");
        var r2 = MakeRecording("clip2.mp4");
        _archiveService.ArchiveAsync(Arg.Any<Recording>(), Arg.Any<IProgress<double>?>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        _viewModel.ToggleMultiSelectCommand.Execute(null);
        _viewModel.ToggleRecordingSelectionCommand.Execute(r1);
        _viewModel.ToggleRecordingSelectionCommand.Execute(r2);

        await _viewModel.ArchiveSelectedCommand.ExecuteAsync(null);

        // After completion, progress should be 1.0
        _viewModel.ArchiveProgress.Should().Be(1.0);
    }

    [Fact]
    public async Task ArchiveSelectedCommand_SetsArchiveStatusText()
    {
        var r1 = MakeRecording("clip1.mp4");
        var capturedStatuses = new List<string>();
        _archiveService.ArchiveAsync(Arg.Any<Recording>(), Arg.Any<IProgress<double>?>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                // Capture the status text when archive is called
                capturedStatuses.Add(_viewModel.ArchiveStatusText);
                return Task.CompletedTask;
            });

        _viewModel.ToggleMultiSelectCommand.Execute(null);
        _viewModel.ToggleRecordingSelectionCommand.Execute(r1);

        await _viewModel.ArchiveSelectedCommand.ExecuteAsync(null);

        // The status text was set to "Archiving clip N of M..." during the loop
        capturedStatuses.Should().ContainMatch("Archiving clip*");
    }

    [Fact]
    public async Task AfterArchive_IsArchivedFlagCheckedViaArchiveService()
    {
        var r1 = MakeRecording("archived.mp4");
        _archiveService.ArchiveAsync(Arg.Any<Recording>(), Arg.Any<IProgress<double>?>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        _archiveService.IsArchived(r1).Returns(true);

        _viewModel.ToggleMultiSelectCommand.Execute(null);
        _viewModel.ToggleRecordingSelectionCommand.Execute(r1);

        await _viewModel.ArchiveSelectedCommand.ExecuteAsync(null);

        _viewModel.ArchivedFileNames.Should().Contain("archived.mp4");
    }

    // ── CancelArchive Tests ───────────────────────────────────────────────────

    [Fact]
    public async Task CancelArchiveCommand_CancelsArchiveOperation()
    {
        var r1 = MakeRecording("clip1.mp4");
        var archiveStarted = new TaskCompletionSource();
        var archiveUnblocked = new TaskCompletionSource();

        _archiveService.ArchiveAsync(Arg.Any<Recording>(), Arg.Any<IProgress<double>?>(), Arg.Any<CancellationToken>())
            .Returns(async callInfo =>
            {
                archiveStarted.TrySetResult();
                var ct = callInfo.Arg<CancellationToken>();
                await Task.Delay(Timeout.InfiniteTimeSpan, ct);
            });

        _viewModel.ToggleMultiSelectCommand.Execute(null);
        _viewModel.ToggleRecordingSelectionCommand.Execute(r1);

        var archiveTask = _viewModel.ArchiveSelectedCommand.ExecuteAsync(null);
        await archiveStarted.Task;

        _viewModel.CancelArchiveCommand.Execute(null);
        await archiveTask;

        _viewModel.IsArchiving.Should().BeFalse();
        _viewModel.ArchiveStatusText.Should().Contain("cancel");
    }
}
