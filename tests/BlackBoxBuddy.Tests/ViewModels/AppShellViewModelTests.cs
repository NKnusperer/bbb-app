using BlackBoxBuddy.Models;
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
    private readonly AppShellViewModel _viewModel;

    public AppShellViewModelTests()
    {
        _deviceService = Substitute.For<IDeviceService>();
        _navigationService = Substitute.For<INavigationService>();
        _dialogService = Substitute.For<IDialogService>();
        _deviceService.ConnectionState.Returns(ConnectionState.Disconnected);
        _deviceService.ConnectedDevice.Returns((DeviceInfo?)null);
        var dashboardVm = new DashboardViewModel();
        var recordingsVm = new RecordingsViewModel();
        var liveFeedVm = new LiveFeedViewModel();
        var settingsVm = new SettingsViewModel();
        _viewModel = new AppShellViewModel(
            _deviceService, _navigationService, _dialogService,
            dashboardVm, recordingsVm, liveFeedVm, settingsVm);
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
}
