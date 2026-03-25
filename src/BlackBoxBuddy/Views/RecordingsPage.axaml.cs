using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using BlackBoxBuddy.ViewModels;

namespace BlackBoxBuddy.Views;

public partial class RecordingsPage : ContentPage
{
    protected override Type StyleKeyOverride => typeof(ContentPage);
    private Control? _detailVideoView;
    private bool _subscribedToVm;

    public RecordingsPage()
    {
        InitializeComponent();
        Loaded += OnLoaded;
        DataContextChanged += (_, _) => TrySubscribeToVm();
    }

    private RecordingsViewModel? Vm => DataContext as RecordingsViewModel;

    private async void OnLoaded(object? sender, RoutedEventArgs e)
    {
        TrySubscribeToVm();

        if (Vm is not null && Vm.LoadRecordingsCommand.CanExecute(null))
            await Vm.LoadRecordingsCommand.ExecuteAsync(null);
    }

    private void TrySubscribeToVm()
    {
        if (Vm is null || _subscribedToVm) return;
        _subscribedToVm = true;

        Vm.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(RecordingsViewModel.ActiveDetailViewModel))
                Dispatcher.UIThread.Post(WireDetailVideoView, DispatcherPriority.Loaded);
        };

        // ActiveDetailViewModel may already be set (e.g. opened from Dashboard
        // before this page was loaded). Wire it now.
        if (Vm.ActiveDetailViewModel is not null)
            Dispatcher.UIThread.Post(WireDetailVideoView, DispatcherPriority.Loaded);
    }

    private void WireDetailVideoView()
    {
        var detailVm = Vm?.ActiveDetailViewModel;
        var host = this.FindControl<ContentControl>("DetailVideoHost");
        if (host is null || detailVm is null) return;

        host.Content = null;
        _detailVideoView = VideoViewHelper.CreateInHost(host);
        VideoViewHelper.SetMediaPlayer(_detailVideoView, detailVm.FrontPlayer);
    }
}
