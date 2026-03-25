using BlackBoxBuddy.Device;
using BlackBoxBuddy.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BlackBoxBuddy.ViewModels;

public partial class LiveFeedViewModel : ViewModelBase, IDisposable
{
    private readonly IDashcamDevice _device;
    private readonly IMediaPlayerService _mediaPlayerService;
    private readonly IDeviceService _deviceService;
    private object? _player;
    private CancellationTokenSource? _streamCts;

    [ObservableProperty] private string _selectedCamera = "front";
    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private bool _isConnectionLost;
    [ObservableProperty] private bool _isStreamActive;

    public object? Player => _player;
    public string Title => "Live Feed";

    public LiveFeedViewModel(
        IDashcamDevice device,
        IMediaPlayerService mediaPlayerService,
        IDeviceService deviceService)
    {
        _device = device;
        _mediaPlayerService = mediaPlayerService;
        _deviceService = deviceService;
    }

    [RelayCommand]
    public async Task StartLiveFeedAsync()
    {
        _streamCts = new CancellationTokenSource();
        IsLoading = true;
        IsConnectionLost = false;
        try
        {
            var uri = await _device.GetStreamUriAsync(SelectedCamera, _streamCts.Token);
            if (uri is null) { IsConnectionLost = true; return; }
            if (_player is null)
            {
                _player = _mediaPlayerService.CreatePlayer();
                OnPropertyChanged(nameof(Player));
                // Yield so the UI thread can run a layout pass — the VideoView's
                // NativeControlHost needs one cycle to create its native window
                // handle before VLC can render into it.
                await Task.Delay(1);
            }
            _mediaPlayerService.Play(_player, uri);
            IsStreamActive = true;
        }
        catch (OperationCanceledException) { /* tab switched away */ }
        catch { IsConnectionLost = true; }
        finally { IsLoading = false; }
    }

    [RelayCommand]
    public void StopLiveFeed()
    {
        _streamCts?.Cancel();
        _streamCts?.Dispose();
        _streamCts = null;
        if (_player is not null) _mediaPlayerService.Stop(_player);
        IsStreamActive = false;
        IsLoading = false;
    }

    [RelayCommand]
    public async Task ToggleCameraAsync(string cameraId)
    {
        SelectedCamera = cameraId;
        StopLiveFeed();
        await StartLiveFeedAsync();
    }

    [RelayCommand]
    public async Task RetryAsync() => await StartLiveFeedAsync();

    public void Dispose()
    {
        StopLiveFeed();
        if (_player is not null)
        {
            _mediaPlayerService.DisposePlayer(_player);
            _player = null;
        }
    }
}
