using System.ComponentModel;
using BlackBoxBuddy.Device;
using BlackBoxBuddy.Models.Settings;
using BlackBoxBuddy.Services;
using CommunityToolkit.Mvvm.Input;

namespace BlackBoxBuddy.ViewModels;

public partial class SettingsViewModel : ViewModelBase
{
    private readonly IDashcamDevice _device;
    private readonly IDialogService _dialogService;

    public SettingsViewModel(IDashcamDevice device, IDialogService dialogService)
    {
        _device = device;
        _dialogService = dialogService;
    }

    // ── WiFi Settings ────────────────────────────────────────────────────────

    [CommunityToolkit.Mvvm.ComponentModel.ObservableProperty]
    private WifiBand _wifiBand;

    [CommunityToolkit.Mvvm.ComponentModel.ObservableProperty]
    private WifiMode _wifiMode;

    [CommunityToolkit.Mvvm.ComponentModel.ObservableProperty]
    private string _wifiSsid = string.Empty;

    [CommunityToolkit.Mvvm.ComponentModel.ObservableProperty]
    private string _wifiPassword = string.Empty;

    // ── Recording Settings ───────────────────────────────────────────────────

    [CommunityToolkit.Mvvm.ComponentModel.ObservableProperty]
    private DrivingMode _drivingMode;

    [CommunityToolkit.Mvvm.ComponentModel.ObservableProperty]
    private ParkingMode _parkingMode;

    // ── Channel Settings ─────────────────────────────────────────────────────

    [CommunityToolkit.Mvvm.ComponentModel.ObservableProperty]
    private RecordingChannels _channels;

    // ── Camera Settings ──────────────────────────────────────────────────────

    [CommunityToolkit.Mvvm.ComponentModel.ObservableProperty]
    private RearOrientation _rearOrientation;

    // ── Sensor Settings ──────────────────────────────────────────────────────

    [CommunityToolkit.Mvvm.ComponentModel.ObservableProperty]
    private int _drivingShockSensitivity = 3;

    [CommunityToolkit.Mvvm.ComponentModel.ObservableProperty]
    private int _parkingShockSensitivity = 3;

    [CommunityToolkit.Mvvm.ComponentModel.ObservableProperty]
    private int _radarSensitivity = 3;

    // ── System Settings ──────────────────────────────────────────────────────

    [CommunityToolkit.Mvvm.ComponentModel.ObservableProperty]
    private bool _gpsEnabled;

    [CommunityToolkit.Mvvm.ComponentModel.ObservableProperty]
    private bool _microphoneEnabled;

    [CommunityToolkit.Mvvm.ComponentModel.ObservableProperty]
    private int _speakerVolume = 3;

    // ── Overlay Settings ─────────────────────────────────────────────────────

    [CommunityToolkit.Mvvm.ComponentModel.ObservableProperty]
    private bool _dateOverlayEnabled;

    [CommunityToolkit.Mvvm.ComponentModel.ObservableProperty]
    private bool _timeOverlayEnabled;

    [CommunityToolkit.Mvvm.ComponentModel.ObservableProperty]
    private bool _gpsPositionOverlayEnabled;

    [CommunityToolkit.Mvvm.ComponentModel.ObservableProperty]
    private bool _speedOverlayEnabled;

    [CommunityToolkit.Mvvm.ComponentModel.ObservableProperty]
    private SpeedUnit _speedUnit;

    // ── State Properties ─────────────────────────────────────────────────────

    [CommunityToolkit.Mvvm.ComponentModel.ObservableProperty]
    private bool _isLoading;

    [CommunityToolkit.Mvvm.ComponentModel.ObservableProperty]
    private string? _loadError;

    [CommunityToolkit.Mvvm.ComponentModel.ObservableProperty]
    private bool _isSaveSuccess;

    [CommunityToolkit.Mvvm.ComponentModel.ObservableProperty]
    private string? _saveError;

    [CommunityToolkit.Mvvm.ComponentModel.ObservableProperty]
    private bool _isSaving;

    // ── Dirty State Tracking ──────────────────────────────────────────────────

    private bool _settingsLoaded;
    private bool _isDirty;

    private static readonly HashSet<string> NonSettingProperties = new()
    {
        nameof(IsLoading), nameof(LoadError), nameof(HasUnsavedChanges),
        nameof(IsSaveSuccess), nameof(SaveError), nameof(IsSaving)
    };

    public string Title => "Settings";

    public bool HasUnsavedChanges => _isDirty;

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);
        if (_settingsLoaded && !NonSettingProperties.Contains(e.PropertyName ?? ""))
        {
            _isDirty = true;
            OnPropertyChanged(new PropertyChangedEventArgs(nameof(HasUnsavedChanges)));
            SaveCommand.NotifyCanExecuteChanged();
        }
    }

    // ── Commands ──────────────────────────────────────────────────────────────

    [RelayCommand]
    private async Task LoadSettingsAsync()
    {
        IsLoading = true;
        LoadError = null;
        _settingsLoaded = false;
        try
        {
            var settings = await _device.GetSettingsAsync();

            WifiBand = settings.Wifi.Band;
            WifiMode = settings.Wifi.Mode;
            WifiSsid = settings.Wifi.Ssid;
            WifiPassword = settings.Wifi.Password;
            DrivingMode = settings.Recording.DrivingMode;
            ParkingMode = settings.Recording.ParkingMode;
            Channels = settings.Channels.Channels;
            RearOrientation = settings.Camera.RearOrientation;
            DrivingShockSensitivity = settings.Sensors.DrivingShockSensitivity;
            ParkingShockSensitivity = settings.Sensors.ParkingShockSensitivity;
            RadarSensitivity = settings.Sensors.RadarSensitivity;
            GpsEnabled = settings.System.GpsEnabled;
            MicrophoneEnabled = settings.System.MicrophoneEnabled;
            SpeakerVolume = settings.System.SpeakerVolume;
            DateOverlayEnabled = settings.Overlays.DateEnabled;
            TimeOverlayEnabled = settings.Overlays.TimeEnabled;
            GpsPositionOverlayEnabled = settings.Overlays.GpsPositionEnabled;
            SpeedOverlayEnabled = settings.Overlays.SpeedEnabled;
            SpeedUnit = settings.Overlays.SpeedUnit;

            _settingsLoaded = true;
            _isDirty = false;
            OnPropertyChanged(new PropertyChangedEventArgs(nameof(HasUnsavedChanges)));
            SaveCommand.NotifyCanExecuteChanged();
        }
        catch (Exception)
        {
            LoadError = "Could not load settings";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand(CanExecute = nameof(HasUnsavedChanges))]
    private async Task SaveAsync()
    {
        IsSaving = true;
        IsSaveSuccess = false;
        SaveError = null;
        try
        {
            var settings = BuildDeviceSettings();
            var success = await _device.ApplySettingsAsync(settings);
            if (success)
            {
                _isDirty = false;
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(HasUnsavedChanges)));
                SaveCommand.NotifyCanExecuteChanged();
                IsSaveSuccess = true;
            }
            else
            {
                SaveError = "Failed to save settings. Try again.";
            }
        }
        catch
        {
            SaveError = "Failed to save settings. Try again.";
        }
        finally
        {
            IsSaving = false;
        }
    }

    private DeviceSettings BuildDeviceSettings() => new(
        Wifi: new WifiSettings(WifiBand, WifiMode, WifiSsid, WifiPassword),
        Recording: new RecordingSettings(DrivingMode, ParkingMode),
        Channels: new ChannelSettings(Channels),
        Camera: new CameraSettings(RearOrientation),
        Sensors: new SensorSettings(DrivingShockSensitivity, ParkingShockSensitivity, RadarSensitivity),
        System: new SystemSettings(GpsEnabled, MicrophoneEnabled, SpeakerVolume),
        Overlays: new OverlaySettings(DateOverlayEnabled, TimeOverlayEnabled, GpsPositionOverlayEnabled, SpeedOverlayEnabled, SpeedUnit));

    [RelayCommand]
    private async Task FactoryResetAsync()
    {
        var confirmed = await _dialogService.ShowConfirmAsync(
            "Factory Reset",
            "This will erase all dashcam settings and cannot be undone. Are you sure?",
            confirmText: "Reset to Factory Defaults",
            cancelText: "Keep My Settings",
            isDestructive: true);
        if (!confirmed) return;

        IsLoading = true;
        await _device.FactoryResetAsync();
        await LoadSettingsAsync();
    }

    [RelayCommand]
    private async Task WipeSdCardAsync()
    {
        var confirmed = await _dialogService.ShowConfirmAsync(
            "Wipe SD Card",
            "This will permanently delete all recordings from the SD card. This cannot be undone. Are you sure?",
            confirmText: "Wipe SD Card",
            cancelText: "Keep My Recordings",
            isDestructive: true);
        if (!confirmed) return;

        IsLoading = true;
        await _device.WipeSdCardAsync();
        IsLoading = false;
    }
}
