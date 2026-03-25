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
        var host = this.FindControl<ContentControl>("VideoViewHost");
        if (host is not null)
            _videoView = VideoViewHelper.CreateInHost(host);

        DataContextChanged += (_, _) => SubscribeToPlayerChanges();
        SubscribeToPlayerChanges();
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

    private void SubscribeToPlayerChanges()
    {
        if (Vm is null) return;
        Vm.PropertyChanged += OnVmPropertyChanged;
        VideoViewHelper.SetMediaPlayer(_videoView, Vm.Player);
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
            VideoViewHelper.SetMediaPlayer(_videoView, Vm?.Player);
    }
}
