using BlackBoxBuddy.Device;
using BlackBoxBuddy.Models.Settings;
using BlackBoxBuddy.Services;
using BlackBoxBuddy.ViewModels;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace BlackBoxBuddy.Tests.ViewModels;

public class SettingsViewModelTests
{
    private readonly IDashcamDevice _device;
    private readonly IDialogService _dialogService;
    private readonly SettingsViewModel _viewModel;

    public SettingsViewModelTests()
    {
        _device = Substitute.For<IDashcamDevice>();
        _dialogService = Substitute.For<IDialogService>();
        _viewModel = new SettingsViewModel(_device, _dialogService);
    }

    private static DeviceSettings CreateDefaultSettings() => new(
        Wifi: new WifiSettings(WifiBand.FiveGHz, WifiMode.AccessPoint, "DashcamAP", ""),
        Recording: new RecordingSettings(DrivingMode.Standard, ParkingMode.Standard),
        Channels: new ChannelSettings(RecordingChannels.FrontAndRear),
        Camera: new CameraSettings(RearOrientation.Normal),
        Sensors: new SensorSettings(3, 3, 3),
        System: new SystemSettings(true, true, 3),
        Overlays: new OverlaySettings(true, true, false, false, SpeedUnit.KilometersPerHour));

    // ── Load Tests ──────────────────────────────────────────────────────────

    [Fact]
    public async Task LoadSettingsAsync_PopulatesAllPropertiesFromDevice()
    {
        var settings = CreateDefaultSettings();
        _device.GetSettingsAsync(Arg.Any<CancellationToken>()).Returns(settings);

        await _viewModel.LoadSettingsCommand.ExecuteAsync(null);

        _viewModel.WifiBand.Should().Be(WifiBand.FiveGHz);
        _viewModel.WifiMode.Should().Be(WifiMode.AccessPoint);
        _viewModel.WifiSsid.Should().Be("DashcamAP");
        _viewModel.WifiPassword.Should().Be("");
        _viewModel.DrivingMode.Should().Be(DrivingMode.Standard);
        _viewModel.ParkingMode.Should().Be(ParkingMode.Standard);
        _viewModel.Channels.Should().Be(RecordingChannels.FrontAndRear);
        _viewModel.RearOrientation.Should().Be(RearOrientation.Normal);
        _viewModel.DrivingShockSensitivity.Should().Be(3);
        _viewModel.ParkingShockSensitivity.Should().Be(3);
        _viewModel.RadarSensitivity.Should().Be(3);
        _viewModel.GpsEnabled.Should().BeTrue();
        _viewModel.MicrophoneEnabled.Should().BeTrue();
        _viewModel.SpeakerVolume.Should().Be(3);
        _viewModel.DateOverlayEnabled.Should().BeTrue();
        _viewModel.TimeOverlayEnabled.Should().BeTrue();
        _viewModel.GpsPositionOverlayEnabled.Should().BeFalse();
        _viewModel.SpeedOverlayEnabled.Should().BeFalse();
        _viewModel.SpeedUnit.Should().Be(SpeedUnit.KilometersPerHour);
    }

    [Fact]
    public async Task LoadSettingsAsync_SetsHasUnsavedChangesToFalse()
    {
        _device.GetSettingsAsync(Arg.Any<CancellationToken>()).Returns(CreateDefaultSettings());

        await _viewModel.LoadSettingsCommand.ExecuteAsync(null);

        _viewModel.HasUnsavedChanges.Should().BeFalse();
    }

    [Fact]
    public async Task LoadSettingsAsync_SetsIsLoadingTrueThenFalse()
    {
        var tcs = new TaskCompletionSource<DeviceSettings>();
        _device.GetSettingsAsync(Arg.Any<CancellationToken>()).Returns(tcs.Task);

        var loadTask = _viewModel.LoadSettingsCommand.ExecuteAsync(null);

        _viewModel.IsLoading.Should().BeTrue();

        tcs.SetResult(CreateDefaultSettings());
        await loadTask;

        _viewModel.IsLoading.Should().BeFalse();
    }

    [Fact]
    public async Task LoadSettingsAsync_WhenThrows_SetsLoadError()
    {
        _device.GetSettingsAsync(Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Network error"));

        await _viewModel.LoadSettingsCommand.ExecuteAsync(null);

        _viewModel.LoadError.Should().NotBeNullOrEmpty();
        _viewModel.IsLoading.Should().BeFalse();
    }

    // ── Dirty State Tests ────────────────────────────────────────────────────

    [Fact]
    public async Task AfterLoad_ChangingWifiBand_SetsHasUnsavedChangesTrue()
    {
        _device.GetSettingsAsync(Arg.Any<CancellationToken>()).Returns(CreateDefaultSettings());
        await _viewModel.LoadSettingsCommand.ExecuteAsync(null);

        _viewModel.WifiBand = WifiBand.TwoPointFourGHz;

        _viewModel.HasUnsavedChanges.Should().BeTrue();
    }

    [Fact]
    public async Task AfterLoad_ChangingDrivingShockSensitivity_SetsHasUnsavedChangesTrue()
    {
        _device.GetSettingsAsync(Arg.Any<CancellationToken>()).Returns(CreateDefaultSettings());
        await _viewModel.LoadSettingsCommand.ExecuteAsync(null);

        _viewModel.DrivingShockSensitivity = 5;

        _viewModel.HasUnsavedChanges.Should().BeTrue();
    }

    [Fact]
    public async Task AfterLoad_ChangingGpsEnabled_SetsHasUnsavedChangesTrue()
    {
        _device.GetSettingsAsync(Arg.Any<CancellationToken>()).Returns(CreateDefaultSettings());
        await _viewModel.LoadSettingsCommand.ExecuteAsync(null);

        _viewModel.GpsEnabled = false;

        _viewModel.HasUnsavedChanges.Should().BeTrue();
    }

    // ── SaveCommand Tests ─────────────────────────────────────────────────────

    [Fact]
    public async Task SaveCommand_CanExecute_FalseWhenNoUnsavedChanges()
    {
        _device.GetSettingsAsync(Arg.Any<CancellationToken>()).Returns(CreateDefaultSettings());
        await _viewModel.LoadSettingsCommand.ExecuteAsync(null);

        _viewModel.SaveCommand.CanExecute(null).Should().BeFalse();
    }

    [Fact]
    public async Task SaveCommand_CanExecute_TrueWhenHasUnsavedChanges()
    {
        _device.GetSettingsAsync(Arg.Any<CancellationToken>()).Returns(CreateDefaultSettings());
        await _viewModel.LoadSettingsCommand.ExecuteAsync(null);

        _viewModel.WifiBand = WifiBand.TwoPointFourGHz;

        _viewModel.SaveCommand.CanExecute(null).Should().BeTrue();
    }

    [Fact]
    public async Task SaveCommand_CallsApplySettingsAsyncWithCurrentPropertyValues()
    {
        _device.GetSettingsAsync(Arg.Any<CancellationToken>()).Returns(CreateDefaultSettings());
        _device.ApplySettingsAsync(Arg.Any<DeviceSettings>(), Arg.Any<CancellationToken>()).Returns(true);
        await _viewModel.LoadSettingsCommand.ExecuteAsync(null);

        _viewModel.WifiBand = WifiBand.TwoPointFourGHz;
        await _viewModel.SaveCommand.ExecuteAsync(null);

        await _device.Received(1).ApplySettingsAsync(
            Arg.Is<DeviceSettings>(s => s.Wifi.Band == WifiBand.TwoPointFourGHz),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SaveCommand_OnSuccess_ResetsHasUnsavedChangesToFalse()
    {
        _device.GetSettingsAsync(Arg.Any<CancellationToken>()).Returns(CreateDefaultSettings());
        _device.ApplySettingsAsync(Arg.Any<DeviceSettings>(), Arg.Any<CancellationToken>()).Returns(true);
        await _viewModel.LoadSettingsCommand.ExecuteAsync(null);

        _viewModel.WifiBand = WifiBand.TwoPointFourGHz;
        await _viewModel.SaveCommand.ExecuteAsync(null);

        _viewModel.HasUnsavedChanges.Should().BeFalse();
    }

    [Fact]
    public async Task SaveCommand_OnSuccess_SetsIsSaveSuccessTrue()
    {
        _device.GetSettingsAsync(Arg.Any<CancellationToken>()).Returns(CreateDefaultSettings());
        _device.ApplySettingsAsync(Arg.Any<DeviceSettings>(), Arg.Any<CancellationToken>()).Returns(true);
        await _viewModel.LoadSettingsCommand.ExecuteAsync(null);

        _viewModel.WifiBand = WifiBand.TwoPointFourGHz;
        await _viewModel.SaveCommand.ExecuteAsync(null);

        _viewModel.IsSaveSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task SaveCommand_OnFailure_SetsSaveError()
    {
        _device.GetSettingsAsync(Arg.Any<CancellationToken>()).Returns(CreateDefaultSettings());
        _device.ApplySettingsAsync(Arg.Any<DeviceSettings>(), Arg.Any<CancellationToken>()).Returns(false);
        await _viewModel.LoadSettingsCommand.ExecuteAsync(null);

        _viewModel.WifiBand = WifiBand.TwoPointFourGHz;
        await _viewModel.SaveCommand.ExecuteAsync(null);

        _viewModel.IsSaveSuccess.Should().BeFalse();
        _viewModel.SaveError.Should().NotBeNullOrEmpty();
    }

    // ── FactoryReset Tests ───────────────────────────────────────────────────

    [Fact]
    public async Task FactoryResetCommand_CallsShowConfirmAsyncWithIsDestructiveTrue()
    {
        _dialogService.ShowConfirmAsync(
            Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<bool>()).Returns(false);

        await _viewModel.FactoryResetCommand.ExecuteAsync(null);

        await _dialogService.Received(1).ShowConfirmAsync(
            Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<string>(),
            isDestructive: true);
    }

    [Fact]
    public async Task FactoryResetCommand_WhenCancelled_DoesNotCallFactoryResetAsync()
    {
        _dialogService.ShowConfirmAsync(
            Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<bool>()).Returns(false);

        await _viewModel.FactoryResetCommand.ExecuteAsync(null);

        await _device.DidNotReceive().FactoryResetAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task FactoryResetCommand_WhenConfirmed_CallsFactoryResetAsync()
    {
        _dialogService.ShowConfirmAsync(
            Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<bool>()).Returns(true);
        _device.FactoryResetAsync(Arg.Any<CancellationToken>()).Returns(true);
        _device.GetSettingsAsync(Arg.Any<CancellationToken>()).Returns(CreateDefaultSettings());

        await _viewModel.FactoryResetCommand.ExecuteAsync(null);

        await _device.Received(1).FactoryResetAsync(Arg.Any<CancellationToken>());
    }

    // ── WipeSdCard Tests ──────────────────────────────────────────────────────

    [Fact]
    public async Task WipeSdCardCommand_CallsShowConfirmAsyncWithIsDestructiveTrue()
    {
        _dialogService.ShowConfirmAsync(
            Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<bool>()).Returns(false);

        await _viewModel.WipeSdCardCommand.ExecuteAsync(null);

        await _dialogService.Received(1).ShowConfirmAsync(
            Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<string>(),
            isDestructive: true);
    }

    [Fact]
    public async Task WipeSdCardCommand_WhenConfirmed_CallsWipeSdCardAsync()
    {
        _dialogService.ShowConfirmAsync(
            Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<bool>()).Returns(true);
        _device.WipeSdCardAsync(Arg.Any<CancellationToken>()).Returns(true);

        await _viewModel.WipeSdCardCommand.ExecuteAsync(null);

        await _device.Received(1).WipeSdCardAsync(Arg.Any<CancellationToken>());
    }
}
