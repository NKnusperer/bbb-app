using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using BlackBoxBuddy.Services;
using BlackBoxBuddy.Navigation;

namespace BlackBoxBuddy.ViewModels.Provisioning;

public partial class ProvisioningViewModel : ViewModelBase
{
    private readonly IDeviceService _deviceService;
    private readonly INavigationService _navigationService;

    // Step 0: Welcome, Step 1: WiFi Setup, Step 2: Confirmation
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(BackCommand))]
    [NotifyCanExecuteChangedFor(nameof(NextCommand))]
    private int _currentStep;

    [ObservableProperty]
    private string _selectedWifiMode = "ap"; // "ap" or "client"

    [ObservableProperty]
    private string _networkName = string.Empty;

    [ObservableProperty]
    private string _networkPassword = string.Empty;

    [ObservableProperty]
    private bool _isProvisioning;

    [ObservableProperty]
    private string? _errorMessage;

    public string DeviceName => _deviceService.ConnectedDevice?.DeviceName ?? "Unknown Device";
    public string FirmwareVersion => _deviceService.ConnectedDevice?.FirmwareVersion ?? "Unknown";

    public int TotalSteps => 3;
    public bool IsWelcomeStep => CurrentStep == 0;
    public bool IsWifiStep => CurrentStep == 1;
    public bool IsConfirmationStep => CurrentStep == 2;

    public ProvisioningViewModel(IDeviceService deviceService, INavigationService navigationService)
    {
        _deviceService = deviceService;
        _navigationService = navigationService;
    }

    [RelayCommand(CanExecute = nameof(CanGoNext))]
    private void Next()
    {
        CurrentStep++;
        OnPropertyChanged(nameof(IsWelcomeStep));
        OnPropertyChanged(nameof(IsWifiStep));
        OnPropertyChanged(nameof(IsConfirmationStep));
    }

    private bool CanGoNext() => CurrentStep < TotalSteps - 1;

    [RelayCommand(CanExecute = nameof(CanGoBack))]
    private void Back()
    {
        CurrentStep--;
        OnPropertyChanged(nameof(IsWelcomeStep));
        OnPropertyChanged(nameof(IsWifiStep));
        OnPropertyChanged(nameof(IsConfirmationStep));
    }

    private bool CanGoBack() => CurrentStep > 0;

    [RelayCommand]
    private async Task CompleteAsync()
    {
        IsProvisioning = true;
        ErrorMessage = null;
        try
        {
            var settings = new Dictionary<string, object>
            {
                ["wifiMode"] = SelectedWifiMode,
                ["networkName"] = NetworkName,
                ["networkPassword"] = NetworkPassword
            };

            var success = await _deviceService.ProvisionAsync(settings);
            if (success)
            {
                // Navigate to Dashboard per D-19
                await _navigationService.PopToRootAsync();
            }
            else
            {
                ErrorMessage = "Provisioning failed. Please try again.";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Provisioning failed: {ex.Message}";
        }
        finally
        {
            IsProvisioning = false;
        }
    }
}
