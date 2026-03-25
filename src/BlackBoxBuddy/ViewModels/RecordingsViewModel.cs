using System.Collections.ObjectModel;
using BlackBoxBuddy.Device;
using BlackBoxBuddy.Models;
using BlackBoxBuddy.Navigation;
using BlackBoxBuddy.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BlackBoxBuddy.ViewModels;

public partial class RecordingsViewModel : ViewModelBase
{
    private readonly IDashcamDevice _device;
    private readonly IDeviceService _deviceService;
    private readonly ITripGroupingService _tripGroupingService;
    private readonly IArchiveService _archiveService;
    private readonly INavigationService _navigationService;
    private IReadOnlyList<Recording> _allRecordings = Array.Empty<Recording>();

    // ── Observable Properties ─────────────────────────────────────────────────

    [ObservableProperty]
    private ObservableCollection<object> _displayItems = new();

    [ObservableProperty]
    private EventType? _selectedFilter;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _isEmpty;

    [ObservableProperty]
    private bool _isDeviceConnected;

    [ObservableProperty]
    private bool _hasActiveFilter;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    public string Title => "Recordings";

    public RecordingsViewModel(
        IDashcamDevice device,
        IDeviceService deviceService,
        ITripGroupingService tripGroupingService,
        IArchiveService archiveService,
        INavigationService navigationService)
    {
        _device = device;
        _deviceService = deviceService;
        _tripGroupingService = tripGroupingService;
        _archiveService = archiveService;
        _navigationService = navigationService;

        // Initialize connection state from current device service state
        IsDeviceConnected = _deviceService.ConnectionState == ConnectionState.Connected;

        // Subscribe to connection state changes
        _deviceService.ConnectionStateChanged += OnConnectionStateChanged;
    }

    private void OnConnectionStateChanged(object? sender, ConnectionState state)
    {
        var wasConnected = IsDeviceConnected;
        IsDeviceConnected = state == ConnectionState.Connected;

        // Auto-reload when device connects
        if (!wasConnected && IsDeviceConnected)
        {
            _ = LoadRecordingsCommand.ExecuteAsync(null);
        }
    }

    // ── Commands ──────────────────────────────────────────────────────────────

    [RelayCommand]
    private async Task LoadRecordingsAsync(CancellationToken ct = default)
    {
        IsLoading = true;
        ErrorMessage = string.Empty;
        try
        {
            _allRecordings = await _device.ListRecordingsAsync(ct);
            ApplyFilter();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Could not load recordings: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void SetFilter(EventType? filter)
    {
        SelectedFilter = filter;
        ApplyFilter();
    }

    [RelayCommand]
    private async Task OpenRecordingAsync(Recording recording)
    {
        var detailVm = new RecordingDetailViewModel(recording);
        await _navigationService.PushAsync(detailVm);
    }

    [RelayCommand]
    private async Task OpenTripAsync(TripGroup trip)
    {
        var detailVm = new RecordingDetailViewModel(trip);
        await _navigationService.PushAsync(detailVm);
    }

    // ── Private Methods ───────────────────────────────────────────────────────

    private void ApplyFilter()
    {
        IReadOnlyList<Recording> filtered = SelectedFilter is null
            ? _allRecordings
            : _allRecordings.Where(r => r.EventType == SelectedFilter.Value).ToList();

        var grouped = _tripGroupingService.Group(filtered);

        DisplayItems.Clear();
        foreach (var item in grouped)
            DisplayItems.Add(item);

        IsEmpty = DisplayItems.Count == 0;
        HasActiveFilter = SelectedFilter is not null;
    }

    partial void OnSelectedFilterChanged(EventType? value)
    {
        ApplyFilter();
    }
}
