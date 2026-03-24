using Avalonia.Controls;
using CommunityToolkit.Mvvm.DependencyInjection;
using BlackBoxBuddy.ViewModels;

namespace BlackBoxBuddy.Views;

public partial class LiveFeedPage : ContentPage
{
    public LiveFeedPage()
    {
        InitializeComponent();
        DataContext = Ioc.Default.GetRequiredService<LiveFeedViewModel>();
    }
}
