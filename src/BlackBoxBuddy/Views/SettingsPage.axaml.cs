using Avalonia.Controls;
using Avalonia.Interactivity;
using BlackBoxBuddy.ViewModels;

namespace BlackBoxBuddy.Views;

public partial class SettingsPage : ContentPage
{
    protected override Type StyleKeyOverride => typeof(ContentPage);

    public SettingsPage()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        if (DataContext is SettingsViewModel vm && vm.LoadSettingsCommand.CanExecute(null))
        {
            vm.LoadSettingsCommand.Execute(null);
        }
    }
}
