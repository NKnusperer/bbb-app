using System.Collections.ObjectModel;
using BlackBoxBuddy.Device;
using BlackBoxBuddy.Models;
using BlackBoxBuddy.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BlackBoxBuddy.ViewModels;

public partial class DashboardViewModel : ViewModelBase
{
    private readonly IDashcamDevice _device;
    private readonly IDeviceService _deviceService;
    private readonly ITripGroupingService _tripGroupingService;
    private readonly Action<int> _switchTab;
    private readonly Action<EventType?> _applyFilter;
    private readonly Action<Recording> _openRecording;
    private readonly Action<TripGroup> _openTrip;

    [ObservableProperty] private bool _isDashboardLoaded;
    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private bool _isDeviceConnected;

    public bool IsEmptyState => !IsDeviceConnected && !IsDashboardLoaded;

    partial void OnIsDeviceConnectedChanged(bool value) => OnPropertyChanged(nameof(IsEmptyState));
    partial void OnIsDashboardLoadedChanged(bool value) => OnPropertyChanged(nameof(IsEmptyState));

    public ObservableCollection<Recording> RecentRecordings { get; } = new();
    public ObservableCollection<TripGroup> RecentTrips { get; } = new();
    public ObservableCollection<Recording> RecentEvents { get; } = new();

    public string Title => "Dashboard";

    public DashboardViewModel(
        IDashcamDevice device,
        IDeviceService deviceService,
        ITripGroupingService tripGroupingService,
        Action<int> switchTab,
        Action<EventType?> applyFilter,
        Action<Recording> openRecording,
        Action<TripGroup> openTrip)
    {
        _device = device;
        _deviceService = deviceService;
        _tripGroupingService = tripGroupingService;
        _switchTab = switchTab;
        _applyFilter = applyFilter;
        _openRecording = openRecording;
        _openTrip = openTrip;

        IsDeviceConnected = _deviceService.ConnectionState == ConnectionState.Connected;
        _deviceService.ConnectionStateChanged += (_, state) =>
            IsDeviceConnected = state == ConnectionState.Connected;
    }

    [RelayCommand]
    public async Task LoadDashboardAsync(CancellationToken ct = default)
    {
        if (IsDashboardLoaded) return;
        IsLoading = true;
        try
        {
            var all = await _device.ListRecordingsAsync(ct);
            var grouped = _tripGroupingService.Group(all);

            RecentRecordings.Clear();
            foreach (var item in grouped.OfType<Recording>().Take(5))
                RecentRecordings.Add(item);

            RecentTrips.Clear();
            foreach (var item in grouped.OfType<TripGroup>().Take(5))
                RecentTrips.Add(item);

            RecentEvents.Clear();
            foreach (var item in all.Where(r => r.EventType != EventType.None)
                                   .OrderByDescending(r => r.DateTime)
                                   .Take(5))
                RecentEvents.Add(item);

            IsDashboardLoaded = true;
        }
        catch (OperationCanceledException) { }
        finally { IsLoading = false; }
    }

    [RelayCommand]
    public void SeeAllRecordings() { _switchTab(1); _applyFilter(null); }

    [RelayCommand]
    public void SeeAllTrips() { _switchTab(1); _applyFilter(null); }

    [RelayCommand]
    public void SeeAllEvents() { _switchTab(1); _applyFilter(EventType.GShock); }

    [RelayCommand]
    public void OpenRecording(Recording recording) => _openRecording(recording);

    [RelayCommand]
    public void OpenTrip(TripGroup trip) => _openTrip(trip);
}
