using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using BlackBoxBuddy.Services;

namespace BlackBoxBuddy.ViewModels.Shell;

public partial class ManualConnectionViewModel : ViewModelBase
{
    private readonly IDeviceService _deviceService;
    private readonly Action _onClose;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ConnectCommand))]
    private string _host = string.Empty;

    [ObservableProperty]
    private bool _isConnecting;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private bool _showSdCardMode;

    public ManualConnectionViewModel(IDeviceService deviceService, Action onClose)
    {
        _deviceService = deviceService;
        _onClose = onClose;
    }

    [RelayCommand(CanExecute = nameof(CanConnect))]
    private async Task ConnectAsync()
    {
        IsConnecting = true;
        ErrorMessage = null;
        try
        {
            var success = await _deviceService.ConnectManuallyAsync(Host);
            if (success)
                _onClose();
            else
                ErrorMessage = "Connection failed. Check the IP address and try again.";
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error: {ex.Message}";
        }
        finally
        {
            IsConnecting = false;
        }
    }

    private bool CanConnect() => !string.IsNullOrWhiteSpace(Host);

    [RelayCommand]
    private void Cancel() => _onClose();

    [RelayCommand]
    private void EnterSdCardMode()
    {
        // CONN-04: SD Card mode entry point — placeholder for now
        ShowSdCardMode = true;
    }
}
