using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using BlackBoxBuddy.Models;
using BlackBoxBuddy.Navigation;
using BlackBoxBuddy.Services;
using BlackBoxBuddy.ViewModels.Provisioning;

namespace BlackBoxBuddy.ViewModels.Shell;

public partial class AppShellViewModel : ViewModelBase
{
    private readonly IDeviceService _deviceService;
    private readonly INavigationService _navigationService;

    [ObservableProperty]
    private ConnectionState _connectionState = ConnectionState.Searching;

    [ObservableProperty]
    private string _connectedDeviceName = string.Empty;

    [ObservableProperty]
    private string _connectionStatusText = "Searching...";

    [ObservableProperty]
    private bool _isManualConnectionVisible;

    [ObservableProperty]
    private ManualConnectionViewModel? _manualConnectionViewModel;

    public AppShellViewModel(IDeviceService deviceService, INavigationService navigationService)
    {
        _deviceService = deviceService;
        _navigationService = navigationService;
        _deviceService.ConnectionStateChanged += OnConnectionStateChanged;
    }

    private void OnConnectionStateChanged(object? sender, ConnectionState state)
    {
        ConnectionState = state;
        ConnectionStatusText = state switch
        {
            ConnectionState.Searching => "Searching...",
            ConnectionState.Connected => _deviceService.ConnectedDevice?.DeviceName ?? "Connected",
            ConnectionState.NeedsProvisioning => _deviceService.ConnectedDevice?.DeviceName ?? "Setup Required",
            ConnectionState.Disconnected => "No device found",
            _ => "Unknown"
        };
        ConnectedDeviceName = _deviceService.ConnectedDevice?.DeviceName ?? string.Empty;

        if (state == ConnectionState.NeedsProvisioning)
        {
            // Navigate to provisioning wizard per PROV-01
            _ = _navigationService.PushAsync(
                new ProvisioningViewModel(_deviceService, _navigationService));
        }
    }

    [RelayCommand]
    private async Task StartDiscoveryAsync()
    {
        await _deviceService.StartDiscoveryAsync();
    }

    [RelayCommand]
    private async Task ConnectionIndicatorTappedAsync()
    {
        // Tap on indicator: when Disconnected, show manual connection dialog (CONN-03)
        if (ConnectionState == ConnectionState.Disconnected)
        {
            IsManualConnectionVisible = true;
            ManualConnectionViewModel = new ManualConnectionViewModel(
                _deviceService,
                () => IsManualConnectionVisible = false);
        }
        await Task.CompletedTask;
    }
}
