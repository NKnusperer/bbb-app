using Avalonia.Controls;

namespace BlackBoxBuddy.Views;

public partial class DashboardPage : ContentPage
{
    protected override Type StyleKeyOverride => typeof(ContentPage);

    public DashboardPage() => InitializeComponent();
}
