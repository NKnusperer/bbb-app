using BlackBoxBuddy.Models;
using FluentAssertions;

namespace BlackBoxBuddy.Tests.Models;

public class RecordingTests
{
    private static Recording CreateRecording(
        string fileName = "CLIP_20260320_120000_F.mp4",
        DateTime? dateTime = null,
        TimeSpan? duration = null,
        long fileSize = 100000,
        double avgSpeed = 60.0,
        double peakGForce = 1.0,
        double distance = 1.0,
        EventType eventType = EventType.None,
        CameraChannel cameraChannel = CameraChannel.Front,
        byte[]? thumbnailData = null)
    {
        return new Recording(
            fileName,
            dateTime ?? new DateTime(2026, 3, 20, 12, 0, 0),
            duration ?? TimeSpan.FromMinutes(2),
            fileSize,
            avgSpeed,
            peakGForce,
            distance,
            eventType,
            cameraChannel,
            thumbnailData ?? new byte[57600]);
    }

    [Fact]
    public void Recording_HasAllRequiredProperties()
    {
        var recording = CreateRecording();

        recording.FileName.Should().Be("CLIP_20260320_120000_F.mp4");
        recording.DateTime.Should().Be(new DateTime(2026, 3, 20, 12, 0, 0));
        recording.Duration.Should().Be(TimeSpan.FromMinutes(2));
        recording.FileSize.Should().Be(100000);
        recording.AvgSpeed.Should().Be(60.0);
        recording.PeakGForce.Should().Be(1.0);
        recording.Distance.Should().Be(1.0);
        recording.EventType.Should().Be(EventType.None);
        recording.CameraChannel.Should().Be(CameraChannel.Front);
        recording.ThumbnailData.Should().HaveCount(57600);
    }

    [Fact]
    public void Recording_ValueEqualityWorks_WhenSameValues()
    {
        var thumbnailData = new byte[57600];
        var dt = new DateTime(2026, 3, 20, 12, 0, 0);
        var duration = TimeSpan.FromMinutes(2);

        var r1 = new Recording("CLIP.mp4", dt, duration, 100000, 60.0, 1.0, 1.0, EventType.None, CameraChannel.Front, thumbnailData);
        var r2 = new Recording("CLIP.mp4", dt, duration, 100000, 60.0, 1.0, 1.0, EventType.None, CameraChannel.Front, thumbnailData);

        r1.Should().Be(r2);
    }

    [Fact]
    public void EventType_HasExactlyFourValues()
    {
        var values = Enum.GetValues<EventType>();

        values.Should().HaveCount(4);
        values.Should().Contain(EventType.None);
        values.Should().Contain(EventType.Radar);
        values.Should().Contain(EventType.GShock);
        values.Should().Contain(EventType.Parking);
    }

    [Fact]
    public void EventType_HasCorrectIntValues()
    {
        ((int)EventType.None).Should().Be(0);
        ((int)EventType.Radar).Should().Be(1);
        ((int)EventType.GShock).Should().Be(2);
        ((int)EventType.Parking).Should().Be(3);
    }

    [Fact]
    public void CameraChannel_HasFrontRearBothValues()
    {
        var values = Enum.GetValues<CameraChannel>();

        values.Should().Contain(CameraChannel.Front);
        values.Should().Contain(CameraChannel.Rear);
        values.Should().Contain(CameraChannel.Both);
    }

    [Fact]
    public void TripGroup_CalculatesTotalDuration_FromSumOfClips()
    {
        var clip1 = CreateRecording(duration: TimeSpan.FromMinutes(2));
        var clip2 = CreateRecording(duration: TimeSpan.FromMinutes(3));
        var trip = new TripGroup(new List<Recording> { clip1, clip2 });

        trip.TotalDuration.Should().Be(TimeSpan.FromMinutes(5));
    }

    [Fact]
    public void TripGroup_CalculatesTotalDistance_FromSumOfClips()
    {
        var clip1 = CreateRecording(distance: 2.5);
        var clip2 = CreateRecording(distance: 3.5);
        var trip = new TripGroup(new List<Recording> { clip1, clip2 });

        trip.TotalDistance.Should().BeApproximately(6.0, 0.001);
    }

    [Fact]
    public void TripGroup_CalculatesAvgSpeed_AsWeightedAverageByDuration()
    {
        // clip1: 60 km/h for 60s, clip2: 120 km/h for 60s => weighted avg = (60*60 + 120*60)/(60+60) = 90
        var clip1 = CreateRecording(avgSpeed: 60.0, duration: TimeSpan.FromSeconds(60));
        var clip2 = CreateRecording(avgSpeed: 120.0, duration: TimeSpan.FromSeconds(60));
        var trip = new TripGroup(new List<Recording> { clip1, clip2 });

        trip.AvgSpeed.Should().BeApproximately(90.0, 0.001);
    }

    [Fact]
    public void TripGroup_CalculatesPeakGForce_AsMaxAcrossClips()
    {
        var clip1 = CreateRecording(peakGForce: 1.5);
        var clip2 = CreateRecording(peakGForce: 3.2);
        var clip3 = CreateRecording(peakGForce: 2.0);
        var trip = new TripGroup(new List<Recording> { clip1, clip2, clip3 });

        trip.PeakGForce.Should().Be(3.2);
    }
}
