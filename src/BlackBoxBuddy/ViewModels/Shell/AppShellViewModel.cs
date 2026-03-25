using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using BlackBoxBuddy.Device;
using BlackBoxBuddy.Models;
using BlackBoxBuddy.Navigation;
using BlackBoxBuddy.Services;
using BlackBoxBuddy.ViewModels.Provisioning;

namespace BlackBoxBuddy.ViewModels.Shell;

public partial class AppShellViewModel : ViewModelBase
{
    private readonly IDeviceService _deviceService;
    private readonly INavigationService _navigationService;

    private const int SettingsTabIndex = 3;
    private bool _isHandlingNavigation;

    [ObservableProperty]
    private int _selectedTabIndex;

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

    public DashboardViewModel DashboardVm { get; }
    public RecordingsViewModel RecordingsVm { get; }
    public LiveFeedViewModel LiveFeedVm { get; }
    public SettingsViewModel SettingsVm { get; }
    public IDialogService DialogService { get; }

    public AppShellViewModel(
        IDeviceService deviceService,
        INavigationService navigationService,
        IDialogService dialogService,
        IDashcamDevice device,
        ITripGroupingService tripGroupingService,
        RecordingsViewModel recordingsVm,
        LiveFeedViewModel liveFeedVm,
        SettingsViewModel settingsVm)
    {
        _deviceService = deviceService;
        _navigationService = navigationService;
        DialogService = dialogService;
        RecordingsVm = recordingsVm;
        LiveFeedVm = liveFeedVm;
        SettingsVm = settingsVm;

        DashboardVm = new DashboardViewModel(
            device, deviceService, tripGroupingService,
            switchTab: idx => SelectedTabIndex = idx,
            applyFilter: filter => RecordingsVm.SetFilterCommand.Execute(filter),
            openRecording: r => { RecordingsVm.OpenRecordingCommand.Execute(r); SelectedTabIndex = 1; },
            openTrip: t => { RecordingsVm.OpenTripCommand.Execute(t); SelectedTabIndex = 1; });

        _deviceService.ConnectionStateChanged += OnConnectionStateChanged;
    }

    partial void OnSelectedTabIndexChanged(int oldValue, int newValue)
    {
        if (_isHandlingNavigation) return;

        // Only guard when leaving Settings tab with unsaved changes
        if (oldValue == SettingsTabIndex && SettingsVm.HasUnsavedChanges)
        {
            _ = HandleUnsavedChangesNavigationAsync(newValue);
        }
    }

    private async Task HandleUnsavedChangesNavigationAsync(int requestedIndex)
    {
        _isHandlingNavigation = true;

        // Revert to Settings tab immediately to prevent navigation
        SelectedTabIndex = SettingsTabIndex;

        var shouldSave = await DialogService.ShowConfirmAsync(
            "Unsaved Settings",
            "You have unsaved settings. What would you like to do?",
            confirmText: "Save & Leave",
            cancelText: "Discard Changes",
            isDestructive: false);

        if (shouldSave)
        {
            if (SettingsVm.SaveCommand.CanExecute(null))
                await SettingsVm.SaveCommand.ExecuteAsync(null);
        }
        else
        {
            SettingsVm.DiscardChanges();
        }

        SelectedTabIndex = requestedIndex;
        _isHandlingNavigation = false;
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
