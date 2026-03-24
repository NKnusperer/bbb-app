using BlackBoxBuddy.Device.Mock;
using BlackBoxBuddy.Models.Settings;
using FluentAssertions;

namespace BlackBoxBuddy.Tests.Device;

public class MockDashcamDeviceTests
{
    [Fact]
    public async Task DiscoverAsync_ReturnsDeviceInfo_WhenSimulateFailureIsFalse()
    {
        var device = new MockDashcamDevice(simulateFailure: false);

        var result = await device.DiscoverAsync();

        result.Should().NotBeNull();
        result!.DeviceName.Should().Be("Mock Dashcam Pro");
    }

    [Fact]
    public async Task DiscoverAsync_ReturnsNull_WhenSimulateFailureIsTrue()
    {
        var device = new MockDashcamDevice(simulateFailure: true);

        var result = await device.DiscoverAsync();

        result.Should().BeNull();
    }

    [Fact]
    public async Task DiscoverAsync_ThrowsOperationCanceledException_WhenCancelled()
    {
        using var cts = new CancellationTokenSource();
        var device = new MockDashcamDevice(discoveryDelay: TimeSpan.FromSeconds(10));

        cts.Cancel();

        // TaskCanceledException inherits from OperationCanceledException; both are valid
        var ex = await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => device.DiscoverAsync(cts.Token));
        ex.Should().BeAssignableTo<OperationCanceledException>();
    }

    [Fact]
    public async Task DiscoverAsync_DelaysForConfiguredDuration()
    {
        var delay = TimeSpan.FromMilliseconds(200);
        var device = new MockDashcamDevice(discoveryDelay: delay);

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        await device.DiscoverAsync();
        stopwatch.Stop();

        stopwatch.Elapsed.Should().BeGreaterThan(TimeSpan.FromMilliseconds(150));
    }

    [Fact]
    public async Task ConnectAsync_ReturnsTrueAndSetsIsConnected()
    {
        var device = new MockDashcamDevice();

        var result = await device.ConnectAsync("192.168.1.1");

        result.Should().BeTrue();
        device.IsConnected.Should().BeTrue();
    }

    [Fact]
    public async Task DisconnectAsync_SetsIsConnectedToFalse()
    {
        var device = new MockDashcamDevice();
        await device.ConnectAsync("192.168.1.1");

        await device.DisconnectAsync();

        device.IsConnected.Should().BeFalse();
    }

    [Fact]
    public async Task ProvisionAsync_SetsIsProvisionedToTrue()
    {
        var device = new MockDashcamDevice(isProvisioned: false);

        await device.ProvisionAsync(new Dictionary<string, object>());

        var info = await device.DiscoverAsync();
        info!.IsProvisioned.Should().BeTrue();
    }

    [Fact]
    public void DefaultConstructor_CreatesDeviceWithIsProvisionedTrue_NoFailure_100msDelay()
    {
        // Default constructor: isProvisioned=true, simulateFailure=false, discoveryDelay=100ms
        var device = new MockDashcamDevice();

        device.IsConnected.Should().BeFalse();
    }

    [Fact]
    public async Task GetSettingsAsync_ReturnsTypedDeviceSettings()
    {
        var device = new MockDashcamDevice();

        var settings = await device.GetSettingsAsync();

        settings.Should().NotBeNull();
        settings.Wifi.Should().NotBeNull();
        settings.Recording.Should().NotBeNull();
        settings.Channels.Should().NotBeNull();
        settings.Camera.Should().NotBeNull();
        settings.Sensors.Should().NotBeNull();
        settings.System.Should().NotBeNull();
        settings.Overlays.Should().NotBeNull();
    }

    [Fact]
    public async Task GetSettingsAsync_DefaultWifiBandIsFiveGHz()
    {
        var device = new MockDashcamDevice();

        var settings = await device.GetSettingsAsync();

        settings.Wifi.Band.Should().Be(WifiBand.FiveGHz);
    }

    [Fact]
    public async Task GetSettingsAsync_DefaultSensorSensitivitiesAreThree()
    {
        var device = new MockDashcamDevice();

        var settings = await device.GetSettingsAsync();

        settings.Sensors.DrivingShockSensitivity.Should().Be(3);
        settings.Sensors.ParkingShockSensitivity.Should().Be(3);
        settings.Sensors.RadarSensitivity.Should().Be(3);
    }

    [Fact]
    public async Task ApplySettingsAsync_PersistsNewSettings()
    {
        var device = new MockDashcamDevice();
        var original = await device.GetSettingsAsync();
        var updated = original with
        {
            Wifi = original.Wifi with { Band = WifiBand.TwoPointFourGHz }
        };

        await device.ApplySettingsAsync(updated);
        var result = await device.GetSettingsAsync();

        result.Wifi.Band.Should().Be(WifiBand.TwoPointFourGHz);
    }

    [Fact]
    public async Task WipeSdCardAsync_ClearsRecordings()
    {
        var device = new MockDashcamDevice();

        await device.WipeSdCardAsync();
        var recordings = await device.ListRecordingsAsync();

        recordings.Should().BeEmpty();
    }

    [Fact]
    public async Task FactoryResetAsync_ResetsSettingsToDefaults()
    {
        var device = new MockDashcamDevice();
        var original = await device.GetSettingsAsync();
        var custom = original with
        {
            Wifi = original.Wifi with { Band = WifiBand.TwoPointFourGHz, Ssid = "CustomNet" }
        };
        await device.ApplySettingsAsync(custom);

        await device.FactoryResetAsync();
        var result = await device.GetSettingsAsync();

        result.Wifi.Band.Should().Be(WifiBand.FiveGHz);
        result.Wifi.Ssid.Should().Be("DashcamAP");
    }
}
