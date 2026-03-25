using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using BlackBoxBuddy.ViewModels;

namespace BlackBoxBuddy.Views;

public partial class LiveFeedPage : ContentPage
{
    protected override Type StyleKeyOverride => typeof(ContentPage);
    private Control? _videoView;

    public LiveFeedPage()
    {
        InitializeComponent();
        IsVisibleProperty.Changed.AddClassHandler<LiveFeedPage>((page, _) => page.OnVisibilityChanged(page.IsVisible));
        Loaded += OnLoaded;
    }

    private LiveFeedViewModel? Vm => DataContext as LiveFeedViewModel;

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        // Create the VideoView early so NativeControlHost has time to create
        // its native window handle before any playback starts.
        EnsureVideoView();
    }

    private void OnVisibilityChanged(bool isVisible)
    {
        if (isVisible)
        {
            Vm?.StartLiveFeedCommand.Execute(null);
            UpdateSegmentStyles();
        }
        else
        {
            Vm?.StopLiveFeedCommand.Execute(null);
        }
    }

    private void EnsureVideoView()
    {
        var host = this.FindControl<ContentControl>("VideoViewHost");
        if (host is null || host.Content is not null) return;

        var videoViewType = Type.GetType(
            "BlackBoxBuddy.Desktop.Controls.VideoView, BlackBoxBuddy.Desktop");
        if (videoViewType is null) return;

        _videoView = Activator.CreateInstance(videoViewType) as Control;
        if (_videoView is null) return;

        host.Content = _videoView;

        // Subscribe to Player property changes to wire MediaPlayer
        DataContextChanged += (_, _) => SubscribeToPlayerChanges();
        SubscribeToPlayerChanges();
    }

    private void SubscribeToPlayerChanges()
    {
        if (Vm is null) return;
        Vm.PropertyChanged += OnVmPropertyChanged;
        // Wire immediately if player already exists
        SetMediaPlayerOnVideoView(_videoView, Vm.Player);
    }

    private static void SetMediaPlayerOnVideoView(object? videoView, object? player)
    {
        if (videoView is null || player is null) return;
        var prop = videoView.GetType().GetProperty("MediaPlayer");
        prop?.SetValue(videoView, player);
    }

    private void UpdateSegmentStyles()
    {
        if (Vm is null) return;

        var frontBtn = this.FindControl<Button>("FrontButton");
        var rearBtn = this.FindControl<Button>("RearButton");
        if (frontBtn is null || rearBtn is null) return;

        var activeBg = new SolidColorBrush(Color.Parse("#2196F3"));
        var inactiveBg = new SolidColorBrush(Colors.Transparent);
        var activeFg = new SolidColorBrush(Colors.White);
        var inactiveFg = new SolidColorBrush(Color.Parse("#B3FFFFFF"));

        bool isFront = Vm.SelectedCamera == "front";
        frontBtn.Background = isFront ? activeBg : inactiveBg;
        frontBtn.Foreground = isFront ? activeFg : inactiveFg;
        rearBtn.Background = !isFront ? activeBg : inactiveBg;
        rearBtn.Foreground = !isFront ? activeFg : inactiveFg;
    }

    private void OnVmPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(LiveFeedViewModel.SelectedCamera))
            UpdateSegmentStyles();
        else if (e.PropertyName == nameof(LiveFeedViewModel.Player))
            SetMediaPlayerOnVideoView(_videoView, Vm?.Player);
    }
}
