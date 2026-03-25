using Avalonia.Controls;
using BlackBoxBuddy.ViewModels;

namespace BlackBoxBuddy.Views;

public partial class RecordingsPage : ContentPage
{
    protected override Type StyleKeyOverride => typeof(ContentPage);

    public RecordingsPage()
    {
        InitializeComponent();
        Loaded += async (_, _) =>
        {
            if (DataContext is RecordingsViewModel vm && vm.LoadRecordingsCommand.CanExecute(null))
                await vm.LoadRecordingsCommand.ExecuteAsync(null);
        };
    }
}
