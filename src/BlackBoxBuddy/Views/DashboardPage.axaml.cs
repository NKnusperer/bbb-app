using Avalonia;
using Avalonia.Controls;
using BlackBoxBuddy.ViewModels;

namespace BlackBoxBuddy.Views;

public partial class DashboardPage : ContentPage
{
    protected override Type StyleKeyOverride => typeof(ContentPage);

    public DashboardPage()
    {
        InitializeComponent();
        IsVisibleProperty.Changed.AddClassHandler<DashboardPage>((page, _) => page.OnVisibilityChanged());
    }

    private DashboardViewModel? Vm => DataContext as DashboardViewModel;

    private void OnVisibilityChanged()
    {
        if (IsVisible)
        {
            // Load dashboard data on first visible (IsDashboardLoaded guard prevents reload)
            Vm?.LoadDashboardCommand.Execute(null);
        }
    }
}
