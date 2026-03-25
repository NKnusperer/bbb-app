using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using BlackBoxBuddy.ViewModels;

namespace BlackBoxBuddy.Views;

public partial class DashboardPage : ContentPage
{
    protected override Type StyleKeyOverride => typeof(ContentPage);

    public DashboardPage()
    {
        InitializeComponent();
        IsVisibleProperty.Changed.AddClassHandler<DashboardPage>((page, _) => page.OnVisibilityChanged());
        Loaded += OnLoaded;
    }

    private DashboardViewModel? Vm => DataContext as DashboardViewModel;

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        // First tab (Dashboard) is visible from construction — IsVisible never
        // changes, so AddClassHandler never fires. Loaded guarantees DataContext
        // is set and the visual tree is ready.
        if (IsVisible)
            Vm?.LoadDashboardCommand.Execute(null);
    }

    private void OnVisibilityChanged()
    {
        if (IsVisible)
        {
            Vm?.LoadDashboardCommand.Execute(null);
        }
    }
}
