using BlackBoxBuddy.Models;
using BlackBoxBuddy.Services;
using FluentAssertions;

namespace BlackBoxBuddy.Tests.Services;

public class TripGroupingServiceTests
{
    private static Recording MakeRecording(DateTime dateTime, TimeSpan duration, string fileName = "")
        => new Recording(
            FileName: string.IsNullOrEmpty(fileName) ? $"rec_{dateTime:HHmmss}.mp4" : fileName,
            DateTime: dateTime,
            Duration: duration,
            FileSize: 1024,
            AvgSpeed: 60,
            PeakGForce: 1.0,
            Distance: 1.0,
            EventType: EventType.None,
            CameraChannel: CameraChannel.Front,
            ThumbnailData: []);

    private readonly ITripGroupingService _sut = new TripGroupingService();
    private static readonly DateTime Base = new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Group_EmptyList_ReturnsEmptyList()
    {
        var result = _sut.Group([]);
        result.Should().BeEmpty();
    }

    [Fact]
    public void Group_SingleRecording_ReturnsOneStandaloneItem()
    {
        var recording = MakeRecording(Base, TimeSpan.FromSeconds(30));
        var result = _sut.Group([recording]);
        result.Should().HaveCount(1);
        result[0].Should().BeOfType<Recording>();
        result[0].Should().Be(recording);
    }

    [Fact]
    public void Group_TwoRecordingsTenSecondsApart_GroupedIntoOneTripGroup()
    {
        // Recording A ends at Base + 30s; Recording B starts at Base + 30s + 10s = Base + 40s
        // Gap = 10s <= 30s => grouped
        var recA = MakeRecording(Base, TimeSpan.FromSeconds(30), "a.mp4");
        var recB = MakeRecording(Base.AddSeconds(40), TimeSpan.FromSeconds(30), "b.mp4");

        var result = _sut.Group([recA, recB]);
        result.Should().HaveCount(1);
        result[0].Should().BeOfType<TripGroup>();
        var trip = (TripGroup)result[0];
        trip.Clips.Should().HaveCount(2);
    }

    [Fact]
    public void Group_TwoRecordingsSixtySecondsApart_RemainTwoStandaloneItems()
    {
        // Recording A ends at Base + 30s; Recording B starts at Base + 30s + 60s = Base + 90s
        // Gap = 60s > 30s => separate
        var recA = MakeRecording(Base, TimeSpan.FromSeconds(30), "a.mp4");
        var recB = MakeRecording(Base.AddSeconds(90), TimeSpan.FromSeconds(30), "b.mp4");

        var result = _sut.Group([recA, recB]);
        result.Should().HaveCount(2);
        result[0].Should().BeOfType<Recording>();
        result[1].Should().BeOfType<Recording>();
    }

    [Fact]
    public void Group_ThreeConsecutivePlusOneIsolated_ReturnsTripGroupAndStandalone()
    {
        // A, B, C are consecutive (10s gap); D is 90s after C ends
        var recA = MakeRecording(Base, TimeSpan.FromSeconds(30), "a.mp4");
        var recB = MakeRecording(Base.AddSeconds(40), TimeSpan.FromSeconds(30), "b.mp4");
        var recC = MakeRecording(Base.AddSeconds(80), TimeSpan.FromSeconds(30), "c.mp4");
        var recD = MakeRecording(Base.AddSeconds(200), TimeSpan.FromSeconds(30), "d.mp4");

        var result = _sut.Group([recA, recB, recC, recD]);
        result.Should().HaveCount(2);

        // Sorted newest-first, so recD comes first as standalone
        result[0].Should().BeOfType<Recording>();
        result[0].Should().Be(recD);

        result[1].Should().BeOfType<TripGroup>();
        var trip = (TripGroup)result[1];
        trip.Clips.Should().HaveCount(3);
    }

    [Fact]
    public void Group_OutputIsSortedNewestFirst()
    {
        var older = MakeRecording(Base, TimeSpan.FromSeconds(30), "older.mp4");
        var newer = MakeRecording(Base.AddSeconds(90), TimeSpan.FromSeconds(30), "newer.mp4");

        var result = _sut.Group([older, newer]); // passed in old order
        result.Should().HaveCount(2);

        // Newest first
        var first = result[0] as Recording;
        first.Should().NotBeNull();
        first!.FileName.Should().Be("newer.mp4");
    }

    [Fact]
    public void Group_GapMeasuredFromEndOfOlderToStartOfNewer()
    {
        // recA: starts at Base, duration 50s -> ends at Base+50s
        // recB: starts at Base+70s -> gap = 70 - 50 = 20s <= 30s => grouped
        var recA = MakeRecording(Base, TimeSpan.FromSeconds(50), "a.mp4");
        var recB = MakeRecording(Base.AddSeconds(70), TimeSpan.FromSeconds(30), "b.mp4");

        var result = _sut.Group([recA, recB]);
        result.Should().HaveCount(1);
        result[0].Should().BeOfType<TripGroup>();
    }

    [Fact]
    public void Group_GapExactlyThirtySeconds_GroupedTogether()
    {
        // recA: starts Base, duration 30s -> ends at Base+30s
        // recB: starts Base+60s -> gap = 30s exactly => grouped (<=30)
        var recA = MakeRecording(Base, TimeSpan.FromSeconds(30), "a.mp4");
        var recB = MakeRecording(Base.AddSeconds(60), TimeSpan.FromSeconds(30), "b.mp4");

        var result = _sut.Group([recA, recB]);
        result.Should().HaveCount(1);
        result[0].Should().BeOfType<TripGroup>();
    }
}
