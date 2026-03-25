using BlackBoxBuddy.Device;
using BlackBoxBuddy.Models;
using BlackBoxBuddy.Navigation;
using BlackBoxBuddy.Services;
using BlackBoxBuddy.ViewModels;
using FluentAssertions;
using NSubstitute;

namespace BlackBoxBuddy.Tests.ViewModels;

public class RecordingsViewModelTests
{
    private readonly IDashcamDevice _device;
    private readonly IDeviceService _deviceService;
    private readonly ITripGroupingService _tripGroupingService;
    private readonly IArchiveService _archiveService;
    private readonly INavigationService _navigationService;
    private readonly IMediaPlayerService _mediaPlayerService;
    private readonly RecordingsViewModel _viewModel;

    public RecordingsViewModelTests()
    {
        _device = Substitute.For<IDashcamDevice>();
        _deviceService = Substitute.For<IDeviceService>();
        _tripGroupingService = Substitute.For<ITripGroupingService>();
        _archiveService = Substitute.For<IArchiveService>();
        _navigationService = Substitute.For<INavigationService>();
        _mediaPlayerService = Substitute.For<IMediaPlayerService>();

        // Default: returns empty list
        _device.ListRecordingsAsync(Arg.Any<CancellationToken>())
            .Returns(Array.Empty<Recording>());
        _tripGroupingService.Group(Arg.Any<IReadOnlyList<Recording>>())
            .Returns(Array.Empty<object>());
        _deviceService.ConnectionState.Returns(ConnectionState.Disconnected);
        _mediaPlayerService.CreatePlayer().Returns(new object());

        _viewModel = new RecordingsViewModel(
            _device, _deviceService, _tripGroupingService, _archiveService, _navigationService, _mediaPlayerService);
    }

    private static Recording MakeRecording(EventType eventType = EventType.None) => new(
        FileName: "clip.mp4",
        DateTime: DateTime.Now,
        Duration: TimeSpan.FromSeconds(30),
        FileSize: 10_485_760,
        AvgSpeed: 60.0,
        PeakGForce: 1.2,
        Distance: 0.5,
        EventType: eventType,
        CameraChannel: CameraChannel.Front,
        ThumbnailData: new byte[160 * 90 * 4]);

    // ── Load Tests ───────────────────────────────────────────────────────────

    [Fact]
    public async Task LoadRecordingsAsync_PopulatesDisplayItemsViaGroupingService()
    {
        var recordings = new[] { MakeRecording(), MakeRecording() };
        var grouped = new object[] { recordings[0], recordings[1] };

        _device.ListRecordingsAsync(Arg.Any<CancellationToken>()).Returns(recordings);
        _tripGroupingService.Group(Arg.Any<IReadOnlyList<Recording>>()).Returns(grouped);

        await _viewModel.LoadRecordingsCommand.ExecuteAsync(null);

        _viewModel.DisplayItems.Should().HaveCount(2);
        _tripGroupingService.Received(1).Group(Arg.Any<IReadOnlyList<Recording>>());
    }

    [Fact]
    public async Task LoadRecordingsAsync_SetsIsLoadingDuringExecution()
    {
        var tcs = new TaskCompletionSource<IReadOnlyList<Recording>>();
        _device.ListRecordingsAsync(Arg.Any<CancellationToken>()).Returns(_ => tcs.Task);

        var loadTask = _viewModel.LoadRecordingsCommand.ExecuteAsync(null);
        _viewModel.IsLoading.Should().BeTrue();

        tcs.SetResult(Array.Empty<Recording>());
        await loadTask;
        _viewModel.IsLoading.Should().BeFalse();
    }

    [Fact]
    public async Task LoadRecordingsAsync_SetsErrorMessageOnFailure()
    {
        _device.ListRecordingsAsync(Arg.Any<CancellationToken>())
            .Returns<IReadOnlyList<Recording>>(_ => throw new Exception("Device error"));

        await _viewModel.LoadRecordingsCommand.ExecuteAsync(null);

        _viewModel.ErrorMessage.Should().NotBeEmpty();
        _viewModel.IsLoading.Should().BeFalse();
    }

    // ── Filter Tests ─────────────────────────────────────────────────────────

    [Fact]
    public async Task WhenSelectedFilterIsNull_AllRecordingsPassedToGroupingService()
    {
        var allRecordings = new[]
        {
            MakeRecording(EventType.None),
            MakeRecording(EventType.Radar),
            MakeRecording(EventType.GShock)
        };
        _device.ListRecordingsAsync(Arg.Any<CancellationToken>()).Returns(allRecordings);

        await _viewModel.LoadRecordingsCommand.ExecuteAsync(null);
        _viewModel.SetFilterCommand.Execute(null);

        _tripGroupingService.Received().Group(
            Arg.Is<IReadOnlyList<Recording>>(r => r.Count == 3));
    }

    [Fact]
    public async Task WhenSelectedFilterIsRadar_OnlyRadarRecordingsPassedToGroupingService()
    {
        var allRecordings = new[]
        {
            MakeRecording(EventType.None),
            MakeRecording(EventType.Radar),
            MakeRecording(EventType.GShock)
        };
        _device.ListRecordingsAsync(Arg.Any<CancellationToken>()).Returns(allRecordings);
        await _viewModel.LoadRecordingsCommand.ExecuteAsync(null);

        _viewModel.SetFilterCommand.Execute(EventType.Radar);

        _tripGroupingService.Received().Group(
            Arg.Is<IReadOnlyList<Recording>>(r => r.Count == 1 && r[0].EventType == EventType.Radar));
    }

    [Fact]
    public async Task WhenSelectedFilterChanges_DisplayItemsIsRebuilt()
    {
        var radarRecording = MakeRecording(EventType.Radar);
        var noneRecording = MakeRecording(EventType.None);
        _device.ListRecordingsAsync(Arg.Any<CancellationToken>())
            .Returns(new[] { radarRecording, noneRecording });
        _tripGroupingService.Group(Arg.Is<IReadOnlyList<Recording>>(r => r.Count == 2))
            .Returns(new object[] { radarRecording, noneRecording });
        _tripGroupingService.Group(Arg.Is<IReadOnlyList<Recording>>(r => r.Count == 1))
            .Returns(new object[] { radarRecording });

        await _viewModel.LoadRecordingsCommand.ExecuteAsync(null);
        _viewModel.DisplayItems.Should().HaveCount(2);

        _viewModel.SetFilterCommand.Execute(EventType.Radar);
        _viewModel.DisplayItems.Should().HaveCount(1);
    }

    // ── IsEmpty Tests ─────────────────────────────────────────────────────────

    [Fact]
    public async Task IsEmpty_TrueWhenDisplayItemsIsEmpty()
    {
        _device.ListRecordingsAsync(Arg.Any<CancellationToken>()).Returns(Array.Empty<Recording>());
        _tripGroupingService.Group(Arg.Any<IReadOnlyList<Recording>>()).Returns(Array.Empty<object>());

        await _viewModel.LoadRecordingsCommand.ExecuteAsync(null);

        _viewModel.IsEmpty.Should().BeTrue();
    }

    [Fact]
    public async Task IsEmpty_FalseWhenDisplayItemsHasItems()
    {
        var recording = MakeRecording();
        _device.ListRecordingsAsync(Arg.Any<CancellationToken>()).Returns(new[] { recording });
        _tripGroupingService.Group(Arg.Any<IReadOnlyList<Recording>>()).Returns(new object[] { recording });

        await _viewModel.LoadRecordingsCommand.ExecuteAsync(null);

        _viewModel.IsEmpty.Should().BeFalse();
    }

    // ── HasActiveFilter Tests ─────────────────────────────────────────────────

    [Fact]
    public void HasActiveFilter_FalseInitially()
    {
        _viewModel.HasActiveFilter.Should().BeFalse();
        _viewModel.SelectedFilter.Should().BeNull();
    }

    [Fact]
    public void HasActiveFilter_TrueWhenFilterIsSet()
    {
        _viewModel.SetFilterCommand.Execute(EventType.Radar);

        _viewModel.HasActiveFilter.Should().BeTrue();
        _viewModel.SelectedFilter.Should().Be(EventType.Radar);
    }

    [Fact]
    public void HasActiveFilter_FalseAfterClearingFilter()
    {
        _viewModel.SetFilterCommand.Execute(EventType.Radar);
        _viewModel.SetFilterCommand.Execute(null);

        _viewModel.HasActiveFilter.Should().BeFalse();
        _viewModel.SelectedFilter.Should().BeNull();
    }

    // ── Navigation Tests ──────────────────────────────────────────────────────

    [Fact]
    public void OpenRecordingCommand_SetsActiveDetailViewModel()
    {
        var recording = MakeRecording();

        _viewModel.OpenRecordingCommand.Execute(recording);

        _viewModel.ActiveDetailViewModel.Should().NotBeNull();
        _viewModel.ActiveDetailViewModel.Should().BeOfType<RecordingDetailViewModel>();
        _viewModel.IsDetailVisible.Should().BeTrue();
    }

    [Fact]
    public void OpenTripCommand_SetsActiveDetailViewModelInTripMode()
    {
        var trip = new TripGroup(new[] { MakeRecording() });

        _viewModel.OpenTripCommand.Execute(trip);

        _viewModel.ActiveDetailViewModel.Should().NotBeNull();
        _viewModel.ActiveDetailViewModel!.IsTrip.Should().BeTrue();
        _viewModel.IsDetailVisible.Should().BeTrue();
    }

    [Fact]
    public void CloseDetailCommand_ClearsActiveDetailViewModel()
    {
        var recording = MakeRecording();
        _viewModel.OpenRecordingCommand.Execute(recording);

        _viewModel.CloseDetailCommand.Execute(null);

        _viewModel.ActiveDetailViewModel.Should().BeNull();
        _viewModel.IsDetailVisible.Should().BeFalse();
    }

    // ── IsDeviceConnected Tests ───────────────────────────────────────────────

    [Fact]
    public void IsDeviceConnected_FalseWhenDeviceServiceReportsDisconnected()
    {
        _deviceService.ConnectionState.Returns(ConnectionState.Disconnected);
        var vm = new RecordingsViewModel(
            _device, _deviceService, _tripGroupingService, _archiveService, _navigationService, _mediaPlayerService);

        vm.IsDeviceConnected.Should().BeFalse();
    }

    [Fact]
    public void IsDeviceConnected_TrueWhenDeviceServiceReportsConnected()
    {
        _deviceService.ConnectionState.Returns(ConnectionState.Connected);
        var vm = new RecordingsViewModel(
            _device, _deviceService, _tripGroupingService, _archiveService, _navigationService, _mediaPlayerService);

        vm.IsDeviceConnected.Should().BeTrue();
    }
}
