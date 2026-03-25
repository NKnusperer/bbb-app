using Avalonia.Controls;
using Avalonia.Interactivity;
using BlackBoxBuddy.ViewModels;

namespace BlackBoxBuddy.Views;

public partial class RecordingDetailPage : ContentPage
{
    private Control? _frontVideoView;
    private Control? _rearVideoView;

    public RecordingDetailPage()
    {
        InitializeComponent();
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    private RecordingDetailViewModel? Vm => DataContext as RecordingDetailViewModel;

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        // Create VideoViews so NativeControlHost has its native handle ready
        // before the user presses Play.
        WireVideoViews();

        if (Vm is not null)
            Vm.InitializePlayback();

        if (this.FindControl<Slider>("SeekSlider") is { } slider)
            slider.AddHandler(PointerReleasedEvent, OnSeekSliderPointerReleased, handledEventsToo: false);
    }

    private void WireVideoViews()
    {
        if (Vm is null) return;

        // Single camera mode — use FrontVideoHost
        var frontHost = this.FindControl<ContentControl>("FrontVideoHost");
        if (frontHost is not null)
        {
            _frontVideoView = VideoViewHelper.CreateInHost(frontHost);
            VideoViewHelper.SetMediaPlayer(_frontVideoView, Vm.FrontPlayer);
        }

        // Dual camera mode — use FrontDualVideoHost + RearVideoHost
        var frontDualHost = this.FindControl<ContentControl>("FrontDualVideoHost");
        if (frontDualHost is not null)
        {
            var dualFront = VideoViewHelper.CreateInHost(frontDualHost);
            VideoViewHelper.SetMediaPlayer(dualFront, Vm.FrontPlayer);
        }

        var rearHost = this.FindControl<ContentControl>("RearVideoHost");
        if (rearHost is not null)
        {
            _rearVideoView = VideoViewHelper.CreateInHost(rearHost);
            VideoViewHelper.SetMediaPlayer(_rearVideoView, Vm.RearPlayer);
        }
    }

    private void OnUnloaded(object? sender, RoutedEventArgs e)
    {
        if (DataContext is IDisposable d)
            d.Dispose();
    }

    private void OnSeekSliderPointerReleased(object? sender, Avalonia.Input.PointerReleasedEventArgs e)
    {
        if (sender is Slider slider && Vm is not null)
            Vm.SeekToCommand.Execute((float)slider.Value);
    }
}
