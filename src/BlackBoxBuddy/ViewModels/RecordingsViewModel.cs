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
    private readonly IMediaPlayerService _mediaPlayerService;
    private IReadOnlyList<Recording> _allRecordings = Array.Empty<Recording>();
    private CancellationTokenSource? _archiveCts;

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

    // ── Multi-select / Archive Properties ─────────────────────────────────────

    [ObservableProperty]
    private bool _isMultiSelectMode;

    [ObservableProperty]
    private ObservableCollection<Recording> _selectedRecordings = new();

    [ObservableProperty]
    private bool _isArchiving;

    [ObservableProperty]
    private double _archiveProgress;

    [ObservableProperty]
    private string _archiveStatusText = string.Empty;

    public int SelectedCount => SelectedRecordings.Count;

    /// <summary>Set of archived file names for quick lookup in UI (archived badge visibility).</summary>
    public HashSet<string> ArchivedFileNames { get; } = new(StringComparer.OrdinalIgnoreCase);

    [ObservableProperty]
    private RecordingDetailViewModel? _activeDetailViewModel;

    public bool IsDetailVisible => ActiveDetailViewModel is not null;

    public string Title => "Recordings";

    public RecordingsViewModel(
        IDashcamDevice device,
        IDeviceService deviceService,
        ITripGroupingService tripGroupingService,
        IArchiveService archiveService,
        INavigationService navigationService,
        IMediaPlayerService mediaPlayerService)
    {
        _device = device;
        _deviceService = deviceService;
        _tripGroupingService = tripGroupingService;
        _archiveService = archiveService;
        _navigationService = navigationService;
        _mediaPlayerService = mediaPlayerService;

        // Initialize connection state from current device service state
        IsDeviceConnected = _deviceService.ConnectionState == ConnectionState.Connected;

        // Subscribe to connection state changes
        _deviceService.ConnectionStateChanged += OnConnectionStateChanged;

        // Track SelectedCount when collection changes
        SelectedRecordings.CollectionChanged += (_, _) => OnPropertyChanged(nameof(SelectedCount));
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
            // Populate archived state from service
            RefreshArchivedState();
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
    private void OpenRecording(Recording recording)
    {
        ActiveDetailViewModel = new RecordingDetailViewModel(recording, _mediaPlayerService, _archiveService, _navigationService);
        OnPropertyChanged(nameof(IsDetailVisible));
    }

    [RelayCommand]
    private void OpenTrip(TripGroup trip)
    {
        ActiveDetailViewModel = new RecordingDetailViewModel(trip, _mediaPlayerService, _archiveService, _navigationService);
        OnPropertyChanged(nameof(IsDetailVisible));
    }

    [RelayCommand]
    private void CloseDetail()
    {
        if (ActiveDetailViewModel is IDisposable d)
            d.Dispose();
        ActiveDetailViewModel = null;
        OnPropertyChanged(nameof(IsDetailVisible));
        // Refresh archived state in case user archived from detail page
        RefreshArchivedState();
        OnPropertyChanged(nameof(ArchivedFileNames));
    }

    // ── Multi-select Commands ─────────────────────────────────────────────────

    [RelayCommand]
    private void ToggleMultiSelect()
    {
        IsMultiSelectMode = !IsMultiSelectMode;
        if (!IsMultiSelectMode)
        {
            SelectedRecordings.Clear();
        }
    }

    [RelayCommand]
    private void ToggleRecordingSelection(Recording recording)
    {
        if (SelectedRecordings.Contains(recording))
            SelectedRecordings.Remove(recording);
        else
            SelectedRecordings.Add(recording);
    }

    [RelayCommand]
    private void SelectAll()
    {
        // Flatten DisplayItems — add all Recording objects from DisplayItems
        var recordingsToAdd = new List<Recording>();
        foreach (var item in DisplayItems)
        {
            if (item is Recording r)
                recordingsToAdd.Add(r);
            else if (item is TripGroup trip)
                recordingsToAdd.AddRange(trip.Clips);
        }

        SelectedRecordings.Clear();
        foreach (var r in recordingsToAdd)
            SelectedRecordings.Add(r);
    }

    [RelayCommand]
    private async Task ArchiveSelected()
    {
        _archiveCts = new CancellationTokenSource();
        IsArchiving = true;
        var selected = SelectedRecordings.ToList();
        try
        {
            for (int i = 0; i < selected.Count; i++)
            {
                _archiveCts.Token.ThrowIfCancellationRequested();
                ArchiveStatusText = $"Archiving clip {i + 1} of {selected.Count}...";
                ArchiveProgress = selected.Count > 0 ? (double)i / selected.Count : 0;
                await _archiveService.ArchiveAsync(selected[i], null, _archiveCts.Token);
            }
            ArchiveProgress = 1.0;
            ArchiveStatusText = "Archived successfully";
        }
        catch (OperationCanceledException)
        {
            ArchiveStatusText = "Archive cancelled";
        }
        finally
        {
            IsArchiving = false;
            _archiveCts?.Dispose();
            _archiveCts = null;
            // Refresh archived state for all processed recordings
            foreach (var r in selected)
                if (_archiveService.IsArchived(r))
                    ArchivedFileNames.Add(r.FileName);
            OnPropertyChanged(nameof(ArchivedFileNames));
            IsMultiSelectMode = false;
            SelectedRecordings.Clear();
            OnPropertyChanged(nameof(SelectedCount));
        }
    }

    [RelayCommand]
    private void CancelArchive()
    {
        _archiveCts?.Cancel();
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

    private void RefreshArchivedState()
    {
        ArchivedFileNames.Clear();
        foreach (var recording in _allRecordings)
        {
            if (_archiveService.IsArchived(recording))
                ArchivedFileNames.Add(recording.FileName);
        }
        OnPropertyChanged(nameof(ArchivedFileNames));
    }

    partial void OnSelectedFilterChanged(EventType? value)
    {
        ApplyFilter();
    }
}
