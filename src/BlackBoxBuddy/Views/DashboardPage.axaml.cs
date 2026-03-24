using Avalonia.Controls;
using CommunityToolkit.Mvvm.DependencyInjection;
using BlackBoxBuddy.ViewModels;

namespace BlackBoxBuddy.Views;

public partial class DashboardPage : ContentPage
{
    public DashboardPage()
    {
        InitializeComponent();
        DataContext = Ioc.Default.GetRequiredService<DashboardViewModel>();
    }
}
