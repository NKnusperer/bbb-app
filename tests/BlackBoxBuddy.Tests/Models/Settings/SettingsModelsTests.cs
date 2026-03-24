using BlackBoxBuddy.Models.Settings;
using FluentAssertions;

namespace BlackBoxBuddy.Tests.Models.Settings;

public class SettingsModelsTests
{
    // WifiSettings record equality
    [Fact]
    public void WifiSettings_TwoRecordsWithSameValues_AreEqual()
    {
        var a = new WifiSettings(WifiBand.FiveGHz, WifiMode.AccessPoint, "MyNet", "pass123");
        var b = new WifiSettings(WifiBand.FiveGHz, WifiMode.AccessPoint, "MyNet", "pass123");

        a.Should().Be(b);
    }

    [Fact]
    public void WifiSettings_TwoRecordsWithDifferentBand_AreNotEqual()
    {
        var a = new WifiSettings(WifiBand.TwoPointFourGHz, WifiMode.AccessPoint, "MyNet", "pass");
        var b = new WifiSettings(WifiBand.FiveGHz, WifiMode.AccessPoint, "MyNet", "pass");

        a.Should().NotBe(b);
    }

    // DeviceSettings composite structure
    [Fact]
    public void DeviceSettings_ContainsAllSevenCategoryRecords()
    {
        var settings = new DeviceSettings(
            Wifi: new WifiSettings(WifiBand.FiveGHz, WifiMode.AccessPoint, "Net", ""),
            Recording: new RecordingSettings(DrivingMode.Standard, ParkingMode.Standard),
            Channels: new ChannelSettings(RecordingChannels.FrontAndRear),
            Camera: new CameraSettings(RearOrientation.Normal),
            Sensors: new SensorSettings(3, 3, 3),
            System: new SystemSettings(true, true, 3),
            Overlays: new OverlaySettings(true, true, false, false, SpeedUnit.KilometersPerHour)
        );

        settings.Wifi.Should().NotBeNull();
        settings.Recording.Should().NotBeNull();
        settings.Channels.Should().NotBeNull();
        settings.Camera.Should().NotBeNull();
        settings.Sensors.Should().NotBeNull();
        settings.System.Should().NotBeNull();
        settings.Overlays.Should().NotBeNull();
    }

    // WifiBand enum members
    [Fact]
    public void WifiBand_HasExpectedMembers()
    {
        var values = Enum.GetValues<WifiBand>();
        values.Should().Contain(WifiBand.TwoPointFourGHz);
        values.Should().Contain(WifiBand.FiveGHz);
    }

    // WifiMode enum members
    [Fact]
    public void WifiMode_HasExpectedMembers()
    {
        var values = Enum.GetValues<WifiMode>();
        values.Should().Contain(WifiMode.AccessPoint);
        values.Should().Contain(WifiMode.Client);
    }

    // DrivingMode enum members
    [Fact]
    public void DrivingMode_HasExpectedMembers()
    {
        var values = Enum.GetValues<DrivingMode>();
        values.Should().Contain(DrivingMode.Standard);
        values.Should().Contain(DrivingMode.Racing);
    }

    // ParkingMode enum members
    [Fact]
    public void ParkingMode_HasExpectedMembers()
    {
        var values = Enum.GetValues<ParkingMode>();
        values.Should().Contain(ParkingMode.Standard);
        values.Should().Contain(ParkingMode.EventOnly);
    }

    // RecordingChannels enum members
    [Fact]
    public void RecordingChannels_HasExpectedMembers()
    {
        var values = Enum.GetValues<RecordingChannels>();
        values.Should().Contain(RecordingChannels.FrontOnly);
        values.Should().Contain(RecordingChannels.FrontAndRear);
    }

    // RearOrientation enum members
    [Fact]
    public void RearOrientation_HasExpectedMembers()
    {
        var values = Enum.GetValues<RearOrientation>();
        values.Should().Contain(RearOrientation.Normal);
        values.Should().Contain(RearOrientation.Flipped);
    }

    // SpeedUnit enum members
    [Fact]
    public void SpeedUnit_HasExpectedMembers()
    {
        var values = Enum.GetValues<SpeedUnit>();
        values.Should().Contain(SpeedUnit.KilometersPerHour);
        values.Should().Contain(SpeedUnit.MilesPerHour);
    }

    // SensorSettings range concept
    [Fact]
    public void SensorSettings_StoresSensitivityAsInt()
    {
        var sensors = new SensorSettings(DrivingShockSensitivity: 1, ParkingShockSensitivity: 5, RadarSensitivity: 3);

        sensors.DrivingShockSensitivity.Should().Be(1);
        sensors.ParkingShockSensitivity.Should().Be(5);
        sensors.RadarSensitivity.Should().Be(3);
    }

    // SystemSettings SpeakerVolume 0 = disabled
    [Fact]
    public void SystemSettings_SpeakerVolumeZero_MeansDisabled()
    {
        var system = new SystemSettings(GpsEnabled: true, MicrophoneEnabled: true, SpeakerVolume: 0);

        system.SpeakerVolume.Should().Be(0);
        // 0 == disabled: any non-zero value activates speaker
        (system.SpeakerVolume == 0).Should().BeTrue("SpeakerVolume of 0 represents disabled state");
    }

    // Record value equality for DeviceSettings (nested records)
    [Fact]
    public void DeviceSettings_TwoRecordsWithSameValues_AreEqual()
    {
        var a = new DeviceSettings(
            Wifi: new WifiSettings(WifiBand.FiveGHz, WifiMode.AccessPoint, "Net", ""),
            Recording: new RecordingSettings(DrivingMode.Standard, ParkingMode.Standard),
            Channels: new ChannelSettings(RecordingChannels.FrontOnly),
            Camera: new CameraSettings(RearOrientation.Normal),
            Sensors: new SensorSettings(3, 3, 3),
            System: new SystemSettings(true, true, 3),
            Overlays: new OverlaySettings(true, true, false, false, SpeedUnit.KilometersPerHour)
        );
        var b = new DeviceSettings(
            Wifi: new WifiSettings(WifiBand.FiveGHz, WifiMode.AccessPoint, "Net", ""),
            Recording: new RecordingSettings(DrivingMode.Standard, ParkingMode.Standard),
            Channels: new ChannelSettings(RecordingChannels.FrontOnly),
            Camera: new CameraSettings(RearOrientation.Normal),
            Sensors: new SensorSettings(3, 3, 3),
            System: new SystemSettings(true, true, 3),
            Overlays: new OverlaySettings(true, true, false, false, SpeedUnit.KilometersPerHour)
        );

        a.Should().Be(b);
    }
}
