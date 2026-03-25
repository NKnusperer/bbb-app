using BlackBoxBuddy.Device;
using BlackBoxBuddy.Models;
using BlackBoxBuddy.Services;
using BlackBoxBuddy.ViewModels;
using Bogus;
using FluentAssertions;
using NSubstitute;

namespace BlackBoxBuddy.Tests.ViewModels;

public class DashboardViewModelTests
{
    private readonly IDashcamDevice _device;
    private readonly IDeviceService _deviceService;
    private readonly ITripGroupingService _tripGroupingService;

    private int _capturedTabIndex = -1;
    private EventType? _capturedFilter = EventType.None;
    private Recording? _capturedRecording;
    private TripGroup? _capturedTrip;

    public DashboardViewModelTests()
    {
        _device = Substitute.For<IDashcamDevice>();
        _deviceService = Substitute.For<IDeviceService>();
        _tripGroupingService = Substitute.For<ITripGroupingService>();

        _device.ListRecordingsAsync(Arg.Any<CancellationToken>())
            .Returns(Array.Empty<Recording>());
        _tripGroupingService.Group(Arg.Any<IReadOnlyList<Recording>>())
            .Returns(Array.Empty<object>());
        _deviceService.ConnectionState.Returns(ConnectionState.Connected);
    }

    private DashboardViewModel CreateSut() => new(
        _device,
        _deviceService,
        _tripGroupingService,
        switchTab: idx => _capturedTabIndex = idx,
        applyFilter: filter => _capturedFilter = filter,
        openRecording: r => _capturedRecording = r,
        openTrip: t => _capturedTrip = t);

    private static Recording MakeRecording(EventType eventType = EventType.None, DateTime? dt = null) =>
        new(
            FileName: $"clip_{Guid.NewGuid():N}.mp4",
            DateTime: dt ?? DateTime.Now,
            Duration: TimeSpan.FromSeconds(30),
            FileSize: 10_485_760,
            AvgSpeed: 60.0,
            PeakGForce: 1.2,
            Distance: 0.5,
            EventType: eventType,
            CameraChannel: CameraChannel.Front,
            ThumbnailData: Array.Empty<byte>());

    [Fact]
    public async Task LoadDashboard_CallsListRecordingsAndGroup()
    {
        var sut = CreateSut();

        await sut.LoadDashboardAsync();

        await _device.Received(1).ListRecordingsAsync(Arg.Any<CancellationToken>());
        _tripGroupingService.Received(1).Group(Arg.Any<IReadOnlyList<Recording>>());
    }

    [Fact]
    public async Task LoadDashboard_PopulatesRecentRecordingsWithTop5StandaloneRecordings()
    {
        var recordings = Enumerable.Range(0, 8).Select(_ => MakeRecording()).ToList();
        // Group returns 8 standalone recordings
        _device.ListRecordingsAsync(Arg.Any<CancellationToken>()).Returns(recordings);
        _tripGroupingService.Group(Arg.Any<IReadOnlyList<Recording>>())
            .Returns(recordings.Cast<object>().ToArray());
        var sut = CreateSut();

        await sut.LoadDashboardAsync();

        sut.RecentRecordings.Should().HaveCount(5);
    }

    [Fact]
    public async Task LoadDashboard_PopulatesRecentTripsWithTop5TripGroups()
    {
        var clips = Enumerable.Range(0, 2).Select(_ => MakeRecording()).ToList();
        var trips = Enumerable.Range(0, 7).Select(_ => new TripGroup(clips)).ToList();
        _device.ListRecordingsAsync(Arg.Any<CancellationToken>()).Returns(Array.Empty<Recording>());
        _tripGroupingService.Group(Arg.Any<IReadOnlyList<Recording>>())
            .Returns(trips.Cast<object>().ToArray());
        var sut = CreateSut();

        await sut.LoadDashboardAsync();

        sut.RecentTrips.Should().HaveCount(5);
    }

    [Fact]
    public async Task LoadDashboard_PopulatesRecentEventsWithTop5EventRecordingsOrderedByDateDesc()
    {
        var baseTime = new DateTime(2025, 1, 1);
        var events = Enumerable.Range(0, 8)
            .Select(i => MakeRecording(EventType.GShock, baseTime.AddHours(i)))
            .ToList<Recording>();
        var noEvents = new[] { MakeRecording(EventType.None) };
        var all = events.Concat(noEvents).ToList();

        _device.ListRecordingsAsync(Arg.Any<CancellationToken>()).Returns(all);
        _tripGroupingService.Group(Arg.Any<IReadOnlyList<Recording>>())
            .Returns(Array.Empty<object>());
        var sut = CreateSut();

        await sut.LoadDashboardAsync();

        sut.RecentEvents.Should().HaveCount(5);
        // Should be ordered descending by DateTime (newest first)
        sut.RecentEvents[0].DateTime.Should().Be(baseTime.AddHours(7));
        sut.RecentEvents[4].DateTime.Should().Be(baseTime.AddHours(3));
    }

    [Fact]
    public async Task LoadDashboard_SetsIsDashboardLoadedTrue()
    {
        var sut = CreateSut();

        await sut.LoadDashboardAsync();

        sut.IsDashboardLoaded.Should().BeTrue();
    }

    [Fact]
    public async Task LoadDashboard_DoesNotReloadOnSecondCall()
    {
        var sut = CreateSut();
        await sut.LoadDashboardAsync();

        await sut.LoadDashboardAsync();

        await _device.Received(1).ListRecordingsAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public void SeeAllRecordings_InvokesSwitchTabWith1()
    {
        var sut = CreateSut();

        sut.SeeAllRecordings();

        _capturedTabIndex.Should().Be(1);
    }

    [Fact]
    public void SeeAllTrips_InvokesSwitchTabWith1()
    {
        var sut = CreateSut();

        sut.SeeAllTrips();

        _capturedTabIndex.Should().Be(1);
    }

    [Fact]
    public void SeeAllEvents_InvokesSwitchTabWith1AndApplyFilterGShock()
    {
        var sut = CreateSut();

        sut.SeeAllEvents();

        _capturedTabIndex.Should().Be(1);
        _capturedFilter.Should().Be(EventType.GShock);
    }

    [Fact]
    public void OpenRecording_InvokesOpenRecordingCallback()
    {
        var recording = MakeRecording();
        var sut = CreateSut();

        sut.OpenRecording(recording);

        _capturedRecording.Should().Be(recording);
    }

    [Fact]
    public void OpenTrip_InvokesOpenTripCallback()
    {
        var clips = new[] { MakeRecording() };
        var trip = new TripGroup(clips);
        var sut = CreateSut();

        sut.OpenTrip(trip);

        _capturedTrip.Should().Be(trip);
    }
}
