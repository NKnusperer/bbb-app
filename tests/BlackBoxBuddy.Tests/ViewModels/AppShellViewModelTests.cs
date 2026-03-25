using BlackBoxBuddy.Device;
using BlackBoxBuddy.Models;
using BlackBoxBuddy.Models.Settings;
using BlackBoxBuddy.Navigation;
using BlackBoxBuddy.Services;
using BlackBoxBuddy.ViewModels;
using BlackBoxBuddy.ViewModels.Shell;
using FluentAssertions;
using NSubstitute;

namespace BlackBoxBuddy.Tests.ViewModels;

public class AppShellViewModelTests
{
    private readonly IDeviceService _deviceService;
    private readonly INavigationService _navigationService;
    private readonly IDialogService _dialogService;
    private readonly IDashcamDevice _device;
    private readonly SettingsViewModel _settingsVm;
    private readonly AppShellViewModel _viewModel;

    public AppShellViewModelTests()
    {
        _deviceService = Substitute.For<IDeviceService>();
        _navigationService = Substitute.For<INavigationService>();
        _dialogService = Substitute.For<IDialogService>();
        _deviceService.ConnectionState.Returns(ConnectionState.Disconnected);
        _deviceService.ConnectedDevice.Returns((DeviceInfo?)null);
        var dashboardVm = new DashboardViewModel();
        var tripGroupingService = Substitute.For<ITripGroupingService>();
        var archiveService = Substitute.For<IArchiveService>();
        var recordingsNavService = Substitute.For<INavigationService>();
        var mockDevice = Substitute.For<IDashcamDevice>();
        var recordingsVm = new RecordingsViewModel(
            mockDevice, _deviceService, tripGroupingService, archiveService, recordingsNavService);
        var liveFeedVm = new LiveFeedViewModel();
        _device = Substitute.For<IDashcamDevice>();
        _settingsVm = new SettingsViewModel(_device, _dialogService);
        _viewModel = new AppShellViewModel(
            _deviceService, _navigationService, _dialogService,
            dashboardVm, recordingsVm, liveFeedVm, _settingsVm);
    }

    private static DeviceSettings CreateDefaultSettings() => new(
        Wifi: new WifiSettings(WifiBand.FiveGHz, WifiMode.AccessPoint, "DashcamAP", ""),
        Recording: new RecordingSettings(DrivingMode.Standard, ParkingMode.Standard),
        Channels: new ChannelSettings(RecordingChannels.FrontAndRear),
        Camera: new CameraSettings(RearOrientation.Normal),
        Sensors: new SensorSettings(3, 3, 3),
        System: new SystemSettings(true, true, 3),
        Overlays: new OverlaySettings(true, true, false, false, SpeedUnit.KilometersPerHour));

    private async Task MakeSettingsDirtyAsync()
    {
        var settings = CreateDefaultSettings();
        _device.GetSettingsAsync(Arg.Any<CancellationToken>()).Returns(settings);
        await _settingsVm.LoadSettingsCommand.ExecuteAsync(null);
        // Change a property to trigger dirty state
        _settingsVm.WifiBand = WifiBand.TwoPointFourGHz;
    }

    private void RaiseConnectionStateChanged(ConnectionState state)
    {
        _deviceService.ConnectionStateChanged += Raise.Event<EventHandler<ConnectionState>>(
            _deviceService, state);
    }

    [Fact]
    public void Constructor_SubscribesToConnectionStateChanged()
    {
        // Verify that the constructor subscribed to the event by firing it
        RaiseConnectionStateChanged(ConnectionState.Connected);

        _viewModel.ConnectionState.Should().Be(ConnectionState.Connected);
    }

    [Fact]
    public void OnConnectionStateChanged_Connected_UpdatesConnectionStateProperty()
    {
        var device = new DeviceInfo { DeviceName = "Test Cam", FirmwareVersion = "1.0", IsProvisioned = true };
        _deviceService.ConnectedDevice.Returns(device);

        RaiseConnectionStateChanged(ConnectionState.Connected);

        _viewModel.ConnectionState.Should().Be(ConnectionState.Connected);
    }

    [Fact]
    public void OnConnectionStateChanged_Connected_ShowsDeviceNameInStatusText()
    {
        var device = new DeviceInfo { DeviceName = "Mock Dashcam Pro", FirmwareVersion = "1.0", IsProvisioned = true };
        _deviceService.ConnectedDevice.Returns(device);

        RaiseConnectionStateChanged(ConnectionState.Connected);

        _viewModel.ConnectionStatusText.Should().Be("Mock Dashcam Pro");
    }

    [Fact]
    public void OnConnectionStateChanged_Disconnected_ShowsNoDeviceFoundText()
    {
        RaiseConnectionStateChanged(ConnectionState.Disconnected);

        _viewModel.ConnectionStatusText.Should().Be("No device found");
    }

    [Fact]
    public async Task OnConnectionStateChanged_NeedsProvisioning_CallsPushAsyncWithProvisioningViewModel()
    {
        var device = new DeviceInfo { DeviceName = "Test Cam", FirmwareVersion = "1.0", IsProvisioned = false };
        _deviceService.ConnectedDevice.Returns(device);
        _navigationService.PushAsync(Arg.Any<ViewModelBase>()).Returns(Task.CompletedTask);

        RaiseConnectionStateChanged(ConnectionState.NeedsProvisioning);

        // Allow async navigation task to complete
        await Task.Delay(50);

        await _navigationService.Received(1).PushAsync(Arg.Any<ViewModelBase>());
    }

    [Fact]
    public async Task ConnectionIndicatorTappedCommand_WhenDisconnected_SetsIsManualConnectionVisibleTrue()
    {
        RaiseConnectionStateChanged(ConnectionState.Disconnected);

        await _viewModel.ConnectionIndicatorTappedCommand.ExecuteAsync(null);

        _viewModel.IsManualConnectionVisible.Should().BeTrue();
    }

    [Fact]
    public async Task StartDiscoveryCommand_CallsStartDiscoveryAsync()
    {
        _deviceService.StartDiscoveryAsync(Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);

        await _viewModel.StartDiscoveryCommand.ExecuteAsync(null);

        await _deviceService.Received(1).StartDiscoveryAsync(Arg.Any<CancellationToken>());
    }

    // ── Unsaved Changes Navigation Guard Tests ─────────────────────────────

    [Fact]
    public async Task OnSelectedTabIndexChanged_FromSettingsWithUnsavedChanges_ShowsDialog()
    {
        await MakeSettingsDirtyAsync();
        _dialogService.ShowConfirmAsync(
            Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<bool>())
            .Returns(Task.FromResult(false));

        _viewModel.SelectedTabIndex = 3; // go to Settings
        _viewModel.SelectedTabIndex = 0; // navigate away

        // Allow async navigation handler to complete
        await Task.Delay(100);

        await _dialogService.Received(1).ShowConfirmAsync(
            "Unsaved Settings",
            Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<bool>());
    }

    [Fact]
    public async Task OnSelectedTabIndexChanged_FromSettingsWithNoUnsavedChanges_DoesNotShowDialog()
    {
        // Settings are clean (no load, no dirty state)
        _viewModel.SelectedTabIndex = 3; // go to Settings
        _viewModel.SelectedTabIndex = 0; // navigate away

        await Task.Delay(50);

        await _dialogService.DidNotReceive().ShowConfirmAsync(
            Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<bool>());
    }

    [Fact]
    public async Task OnSelectedTabIndexChanged_FromNonSettingsTab_DoesNotShowDialog()
    {
        await MakeSettingsDirtyAsync();
        _viewModel.SelectedTabIndex = 0; // start at Dashboard
        _viewModel.SelectedTabIndex = 1; // navigate to Recordings (not from Settings)

        await Task.Delay(50);

        await _dialogService.DidNotReceive().ShowConfirmAsync(
            Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<bool>());
    }

    [Fact]
    public async Task HandleUnsavedChanges_UserChoosesSave_CallsSaveCommand()
    {
        await MakeSettingsDirtyAsync();
        _device.ApplySettingsAsync(Arg.Any<DeviceSettings>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(true));
        _dialogService.ShowConfirmAsync(
            Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<bool>())
            .Returns(Task.FromResult(true)); // "Save & Leave"

        _viewModel.SelectedTabIndex = 3; // go to Settings
        _viewModel.SelectedTabIndex = 0; // navigate away

        await Task.Delay(100);

        await _device.Received(1).ApplySettingsAsync(Arg.Any<DeviceSettings>(), Arg.Any<CancellationToken>());
        _settingsVm.HasUnsavedChanges.Should().BeFalse();
    }

    [Fact]
    public async Task HandleUnsavedChanges_UserChoosesDiscard_CallsDiscardChanges()
    {
        await MakeSettingsDirtyAsync();
        _dialogService.ShowConfirmAsync(
            Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<bool>())
            .Returns(Task.FromResult(false)); // "Discard Changes"

        _viewModel.SelectedTabIndex = 3; // go to Settings
        _viewModel.SelectedTabIndex = 0; // navigate away

        await Task.Delay(100);

        _settingsVm.HasUnsavedChanges.Should().BeFalse();
        await _device.DidNotReceive().ApplySettingsAsync(Arg.Any<DeviceSettings>(), Arg.Any<CancellationToken>());
    }
}
