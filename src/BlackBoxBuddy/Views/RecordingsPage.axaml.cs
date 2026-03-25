using Avalonia.Controls;
using BlackBoxBuddy.ViewModels;

namespace BlackBoxBuddy.Views;

public partial class RecordingsPage : ContentPage
{
    protected override Type StyleKeyOverride => typeof(ContentPage);
    private Control? _detailVideoView;

    public RecordingsPage()
    {
        InitializeComponent();
        Loaded += async (_, _) =>
        {
            if (DataContext is RecordingsViewModel vm && vm.LoadRecordingsCommand.CanExecute(null))
                await vm.LoadRecordingsCommand.ExecuteAsync(null);
        };
    }

    private RecordingsViewModel? Vm => DataContext as RecordingsViewModel;

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
        if (Vm is null) return;

        // When the detail overlay appears (ActiveDetailViewModel set),
        // create VideoView and wire the player.
        Vm.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(RecordingsViewModel.ActiveDetailViewModel))
                WireDetailVideoView();
        };
    }

    private void WireDetailVideoView()
    {
        var detailVm = Vm?.ActiveDetailViewModel;
        var host = this.FindControl<ContentControl>("DetailVideoHost");
        if (host is null || detailVm is null) return;

        // Recreate VideoView for each new detail (previous one was for a different player)
        host.Content = null;
        _detailVideoView = VideoViewHelper.CreateInHost(host);
        VideoViewHelper.SetMediaPlayer(_detailVideoView, detailVm.FrontPlayer);
    }
}
