using BlackBoxBuddy.Device.Mock;
using BlackBoxBuddy.Models;
using FluentAssertions;

namespace BlackBoxBuddy.Tests.Device;

public class MockDashcamDeviceRecordingsTests
{
    [Fact]
    public async Task ListRecordingsAsync_Returns18Recordings()
    {
        var device = new MockDashcamDevice();

        var recordings = await device.ListRecordingsAsync();

        recordings.Should().HaveCount(18);
    }

    [Fact]
    public async Task ListRecordingsAsync_FirstSixHaveConsecutiveTimestamps()
    {
        var device = new MockDashcamDevice();

        var recordings = await device.ListRecordingsAsync();

        // First 6 recordings should be 60s apart (consecutive)
        var firstSix = recordings.Take(6).OrderBy(r => r.DateTime).ToList();
        for (int i = 1; i < firstSix.Count; i++)
        {
            var gap = (firstSix[i].DateTime - firstSix[i - 1].DateTime).TotalSeconds;
            gap.Should().BeLessThanOrEqualTo(30,
                because: "first 6 recordings should have timestamps within 30 seconds of each other for trip grouping");
        }
    }

    [Fact]
    public async Task ListRecordingsAsync_HasMixOfEventTypes()
    {
        var device = new MockDashcamDevice();

        var recordings = await device.ListRecordingsAsync();
        var eventTypes = recordings.Select(r => r.EventType).Distinct().ToList();

        eventTypes.Should().Contain(EventType.Radar, because: "mock data should include Radar events");
        eventTypes.Should().Contain(EventType.GShock, because: "mock data should include GShock events");
        eventTypes.Should().Contain(EventType.Parking, because: "mock data should include Parking events");
    }

    [Fact]
    public async Task ListRecordingsAsync_AllRecordingsHaveThumbnailData()
    {
        var device = new MockDashcamDevice();

        var recordings = await device.ListRecordingsAsync();

        foreach (var recording in recordings)
        {
            recording.ThumbnailData.Should().NotBeEmpty();
            recording.ThumbnailData.Should().HaveCount(160 * 90 * 4,
                because: "thumbnails should be 160x90 BGRA8888 format");
        }
    }

    [Fact]
    public async Task WipeSdCardAsync_ClearsAllRecordings()
    {
        var device = new MockDashcamDevice();

        await device.WipeSdCardAsync();
        var recordings = await device.ListRecordingsAsync();

        recordings.Should().BeEmpty();
    }

    [Fact]
    public async Task DownloadFileAsync_ReturnsNonEmptyStream()
    {
        var device = new MockDashcamDevice();

        var stream = await device.DownloadFileAsync("CLIP_20260320_080000_F.mp4");

        stream.Should().NotBeNull();
        stream.Length.Should().BeGreaterThan(0, because: "sample.mp4 should be a non-empty embedded resource");
    }

    [Fact]
    public async Task ListRecordingsAsync_AllFileNamesEndWithMp4()
    {
        var device = new MockDashcamDevice();

        var recordings = await device.ListRecordingsAsync();

        foreach (var recording in recordings)
        {
            recording.FileName.Should().EndWith(".mp4",
                because: "all mock recording file names should be valid MP4 file names");
        }
    }
}
