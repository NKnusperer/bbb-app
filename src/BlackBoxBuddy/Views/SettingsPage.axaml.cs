using Avalonia.Controls;
using CommunityToolkit.Mvvm.DependencyInjection;
using BlackBoxBuddy.ViewModels;

namespace BlackBoxBuddy.Views;

public partial class SettingsPage : ContentPage
{
    public SettingsPage()
    {
        InitializeComponent();
        DataContext = Ioc.Default.GetRequiredService<SettingsViewModel>();
    }
}
