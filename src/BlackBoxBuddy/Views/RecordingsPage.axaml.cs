using Avalonia.Controls;
using CommunityToolkit.Mvvm.DependencyInjection;
using BlackBoxBuddy.ViewModels;

namespace BlackBoxBuddy.Views;

public partial class RecordingsPage : ContentPage
{
    public RecordingsPage()
    {
        InitializeComponent();
        DataContext = Ioc.Default.GetRequiredService<RecordingsViewModel>();
    }
}
