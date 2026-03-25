using BlackBoxBuddy.Models;
using BlackBoxBuddy.Navigation;
using BlackBoxBuddy.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BlackBoxBuddy.ViewModels;

public partial class RecordingDetailViewModel : ViewModelBase, IDisposable
{
    private readonly IMediaPlayerService _mediaPlayerService;
    private readonly IArchiveService _archiveService;
    private readonly INavigationService _navigationService;
    private object? _frontPlayer;
    private object? _rearPlayer;
    private CancellationTokenSource? _archiveCts;

    // ── Observable Properties ─────────────────────────────────────────────────

    [ObservableProperty]
    private Recording? _currentRecording;

    [ObservableProperty]
    private TripGroup? _trip;

    [ObservableProperty]
    private bool _isTrip;

    [ObservableProperty]
    private bool _isDualCamera;

    [ObservableProperty]
    private bool _isPlaying;

    [ObservableProperty]
    private float _playbackRate = 1.0f;

    [ObservableProperty]
    private float _seekPosition;

    [ObservableProperty]
    private long _currentTimeMs;

    [ObservableProperty]
    private long _totalDurationMs;

    [ObservableProperty]
    private bool _isFullscreen;

    [ObservableProperty]
    private bool _isArchived;

    [ObservableProperty]
    private double _archiveProgress;

    [ObservableProperty]
    private bool _isArchiving;

    // ── Computed properties ───────────────────────────────────────────────────

    public string CurrentTimeFormatted => FormatMs(CurrentTimeMs);
    public string TotalDurationFormatted => FormatMs(TotalDurationMs);

    public string PageTitle => IsTrip ? "Trip Details" : "Recording Details";

    public string DisplayDateTime => IsTrip
        ? Trip?.StartTime.ToString("yyyy-MM-dd HH:mm") ?? string.Empty
        : CurrentRecording?.DateTime.ToString("yyyy-MM-dd HH:mm") ?? string.Empty;

    public string DisplayDuration => IsTrip
        ? FormatTimeSpan(Trip?.TotalDuration ?? TimeSpan.Zero)
        : FormatTimeSpan(CurrentRecording?.Duration ?? TimeSpan.Zero);

    public string DisplayFileSize => IsTrip
        ? string.Empty
        : $"{(CurrentRecording?.FileSize ?? 0) / 1_048_576.0:F1} MB";

    public string DisplayAvgSpeed => IsTrip
        ? $"{Trip?.AvgSpeed ?? 0:F0} km/h"
        : $"{CurrentRecording?.AvgSpeed ?? 0:F0} km/h";

    public string DisplayPeakGForce => IsTrip
        ? $"{Trip?.PeakGForce ?? 0:F1}g"
        : $"{CurrentRecording?.PeakGForce ?? 0:F1}g";

    public string DisplayDistance => IsTrip
        ? $"{Trip?.TotalDistance ?? 0:F1} km"
        : $"{CurrentRecording?.Distance ?? 0:F1} km";

    public IReadOnlyList<Recording>? CurrentClips => Trip?.Clips;

    public int ClipCount => Trip?.Clips.Count ?? 0;

    // ── Player exposure ───────────────────────────────────────────────────────

    public object? FrontPlayer => _frontPlayer;
    public object? RearPlayer => _rearPlayer;

    // ── Constructors ──────────────────────────────────────────────────────────

    /// <summary>Single recording mode.</summary>
    public RecordingDetailViewModel(
        Recording recording,
        IMediaPlayerService mediaPlayerService,
        IArchiveService archiveService,
        INavigationService navigationService)
    {
        _mediaPlayerService = mediaPlayerService;
        _archiveService = archiveService;
        _navigationService = navigationService;

        CurrentRecording = recording;
        IsTrip = false;
        IsDualCamera = recording.CameraChannel == CameraChannel.Both;
        TotalDurationMs = (long)recording.Duration.TotalMilliseconds;
        IsArchived = _archiveService.IsArchived(recording);

        _frontPlayer = _mediaPlayerService.CreatePlayer();
        if (IsDualCamera)
            _rearPlayer = _mediaPlayerService.CreatePlayer();
    }

    /// <summary>Trip mode — sets first clip as active recording.</summary>
    public RecordingDetailViewModel(
        TripGroup trip,
        IMediaPlayerService mediaPlayerService,
        IArchiveService archiveService,
        INavigationService navigationService)
    {
        _mediaPlayerService = mediaPlayerService;
        _archiveService = archiveService;
        _navigationService = navigationService;

        Trip = trip;
        IsTrip = true;
        CurrentRecording = trip.Clips.Count > 0 ? trip.Clips[0] : null;
        IsDualCamera = CurrentRecording?.CameraChannel == CameraChannel.Both;
        TotalDurationMs = (long)trip.TotalDuration.TotalMilliseconds;

        _frontPlayer = _mediaPlayerService.CreatePlayer();
        if (IsDualCamera)
            _rearPlayer = _mediaPlayerService.CreatePlayer();
    }

    // ── Public methods ────────────────────────────────────────────────────────

    /// <summary>
    /// Called from the View's code-behind after the VideoView is loaded.
    /// Does not start playback — user must press play.
    /// </summary>
    public void InitializePlayback()
    {
        // Players are already created in constructor.
        // Playback starts when the user presses play.
    }

    // ── Commands ──────────────────────────────────────────────────────────────

    [RelayCommand]
    private void PlayPause()
    {
        IsPlaying = !IsPlaying;
        if (IsPlaying)
        {
            if (CurrentRecording is not null && _frontPlayer is not null)
                _mediaPlayerService.Play(_frontPlayer, new Uri(CurrentRecording.FileName, UriKind.RelativeOrAbsolute));
            if (IsDualCamera && _rearPlayer is not null && CurrentRecording is not null)
                _mediaPlayerService.Play(_rearPlayer, new Uri(CurrentRecording.FileName, UriKind.RelativeOrAbsolute));
        }
        else
        {
            if (_frontPlayer is not null) _mediaPlayerService.Pause(_frontPlayer);
            if (_rearPlayer is not null) _mediaPlayerService.Pause(_rearPlayer);
        }
    }

    [RelayCommand]
    private void NextFrame()
    {
        if (_frontPlayer is not null)
            _mediaPlayerService.NextFrame(_frontPlayer);
    }

    [RelayCommand]
    private void PreviousFrame()
    {
        if (_frontPlayer is not null)
            _mediaPlayerService.PreviousFrame(_frontPlayer);
    }

    [RelayCommand]
    private void SetRate(object? rateParam)
    {
        var rate = rateParam switch
        {
            float f => f,
            string s when float.TryParse(s, System.Globalization.CultureInfo.InvariantCulture, out var parsed) => parsed,
            _ => 1.0f
        };
        PlaybackRate = rate;
        if (_frontPlayer is not null)
            _mediaPlayerService.SetRate(_frontPlayer, rate);
        if (_rearPlayer is not null)
            _mediaPlayerService.SetRate(_rearPlayer, rate);
    }

    [RelayCommand]
    private void SeekTo(object? posParam)
    {
        var position = posParam switch
        {
            float f => f,
            double d => (float)d,
            string s when float.TryParse(s, System.Globalization.CultureInfo.InvariantCulture, out var parsed) => parsed,
            _ => 0f
        };
        SeekPosition = position;
        if (_frontPlayer is not null)
            _mediaPlayerService.Seek(_frontPlayer, position);
        if (_rearPlayer is not null)
            _mediaPlayerService.Seek(_rearPlayer, position);
    }

    [RelayCommand]
    private void ToggleFullscreen()
    {
        IsFullscreen = !IsFullscreen;
    }

    [RelayCommand]
    private async Task ArchiveRecording()
    {
        _archiveCts = new CancellationTokenSource();
        IsArchiving = true;
        try
        {
            var progress = new Progress<double>(p => ArchiveProgress = p);
            if (IsTrip && Trip is not null)
                await _archiveService.ArchiveTripAsync(Trip, progress, _archiveCts.Token);
            else if (CurrentRecording is not null)
                await _archiveService.ArchiveAsync(CurrentRecording, progress, _archiveCts.Token);
            IsArchived = true;
        }
        catch (OperationCanceledException) { /* cancelled by user */ }
        finally
        {
            IsArchiving = false;
            _archiveCts?.Dispose();
            _archiveCts = null;
        }
    }

    [RelayCommand]
    private void CancelArchive()
    {
        _archiveCts?.Cancel();
    }

    [RelayCommand]
    private async Task GoBack()
    {
        await _navigationService.PopAsync();
    }

    // ── IDisposable ───────────────────────────────────────────────────────────

    public void Dispose()
    {
        _archiveCts?.Cancel();
        _archiveCts?.Dispose();
        if (_frontPlayer is not null) _mediaPlayerService.DisposePlayer(_frontPlayer);
        if (_rearPlayer is not null) _mediaPlayerService.DisposePlayer(_rearPlayer);
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    private static string FormatMs(long ms)
    {
        var ts = TimeSpan.FromMilliseconds(ms);
        return $"{(int)ts.TotalMinutes:D2}:{ts.Seconds:D2}";
    }

    private static string FormatTimeSpan(TimeSpan ts)
    {
        return $"{(int)ts.TotalMinutes:D2}:{ts.Seconds:D2}";
    }
}
