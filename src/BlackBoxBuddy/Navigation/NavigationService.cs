using Avalonia.Controls;
using BlackBoxBuddy.ViewModels;

namespace BlackBoxBuddy.Navigation;

public class NavigationService : INavigationService
{
    private NavigationPage? _navigationPage;
    private readonly IServiceProvider _serviceProvider;

    public NavigationService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public void SetNavigationPage(NavigationPage page)
        => _navigationPage = page;

    public async Task PushAsync(ViewModelBase viewModel)
    {
        if (_navigationPage is null) return;
        var page = new ContentPage { DataContext = viewModel };
        await _navigationPage.PushAsync(page);
    }

    public async Task PopAsync()
    {
        if (_navigationPage is null) return;
        await _navigationPage.PopAsync();
    }

    public async Task PopToRootAsync()
    {
        if (_navigationPage is null) return;
        await _navigationPage.PopToRootAsync();
    }
}
