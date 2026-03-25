using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using BlackBoxBuddy.ViewModels;

namespace BlackBoxBuddy.Views;

public partial class LiveFeedPage : ContentPage
{
    protected override Type StyleKeyOverride => typeof(ContentPage);

    public LiveFeedPage()
    {
        InitializeComponent();
        // Subscribe to IsVisible changes for tab lifecycle.
        // ContentPage inside TabbedPage gets IsVisible toggled on tab switch
        // (not detached/reattached from visual tree). Per 04-RESEARCH.md Pitfall 3.
        IsVisibleProperty.Changed.AddClassHandler<LiveFeedPage>((page, _) => page.OnVisibilityChanged(page.IsVisible));
    }

    private LiveFeedViewModel? Vm => DataContext as LiveFeedViewModel;

    private void OnVisibilityChanged(bool isVisible)
    {
        if (isVisible)
        {
            // Tab appeared — start live feed
            Vm?.StartLiveFeedCommand.Execute(null);
            UpdateSegmentStyles();

            // Wire VideoView MediaPlayer handle if player exists
            WireVideoView();
        }
        else
        {
            // Tab disappeared — stop live feed to release GPU resources
            Vm?.StopLiveFeedCommand.Execute(null);
        }
    }

    private void WireVideoView()
    {
        // VideoView is Desktop-only (BlackBoxBuddy.Desktop.Controls.VideoView).
        // At runtime on Desktop, we create it dynamically and set its MediaPlayer.
        // The ContentControl "VideoViewHost" in XAML is the placeholder.
        var host = this.FindControl<ContentControl>("VideoViewHost");
        if (host is null) return;

        // If already wired, just update MediaPlayer
        if (host.Content is not null)
        {
            SetMediaPlayerOnVideoView(host.Content, Vm?.Player);
            return;
        }

        // Try to create the Desktop VideoView via reflection to avoid
        // compile-time dependency on Desktop project from shared project.
        // This is the standard cross-platform NativeControlHost pattern.
        var videoViewType = Type.GetType(
            "BlackBoxBuddy.Desktop.Controls.VideoView, BlackBoxBuddy.Desktop");
        if (videoViewType is null) return;

        var videoView = Activator.CreateInstance(videoViewType) as Control;
        if (videoView is null) return;

        host.Content = videoView;

        // Subscribe to Player property changes so we can update MediaPlayer
        if (Vm is not null)
        {
            Vm.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(LiveFeedViewModel.Player))
                    SetMediaPlayerOnVideoView(host.Content, Vm.Player);
            };
        }

        SetMediaPlayerOnVideoView(videoView, Vm?.Player);
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
        var inactiveFg = new SolidColorBrush(Color.Parse("#B3FFFFFF")); // White at 0.7

        bool isFront = Vm.SelectedCamera == "front";
        frontBtn.Background = isFront ? activeBg : inactiveBg;
        frontBtn.Foreground = isFront ? activeFg : inactiveFg;
        rearBtn.Background = !isFront ? activeBg : inactiveBg;
        rearBtn.Foreground = !isFront ? activeFg : inactiveFg;

        // Subscribe to SelectedCamera changes for ongoing updates
        Vm.PropertyChanged -= OnVmPropertyChanged;
        Vm.PropertyChanged += OnVmPropertyChanged;
    }

    private void OnVmPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(LiveFeedViewModel.SelectedCamera))
            UpdateSegmentStyles();
        else if (e.PropertyName == nameof(LiveFeedViewModel.Player))
            WireVideoView();
    }
}
