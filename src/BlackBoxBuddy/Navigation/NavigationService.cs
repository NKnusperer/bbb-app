using Avalonia.Controls;
using Avalonia.Controls.Templates;
using BlackBoxBuddy.ViewModels;

namespace BlackBoxBuddy.Navigation;

public class NavigationService : INavigationService
{
    private NavigationPage? _navigationPage;
    private readonly IDataTemplate _viewLocator;

    public NavigationService(IDataTemplate viewLocator)
    {
        _viewLocator = viewLocator;
    }

    public void SetNavigationPage(NavigationPage page)
        => _navigationPage = page;

    public async Task PushAsync(ViewModelBase viewModel)
    {
        if (_navigationPage is null) return;

        // Resolve the page via ViewLocator so page-specific code-behind
        // (VideoView wiring, lifecycle hooks, etc.) is used.
        var view = _viewLocator.Build(viewModel);
        ContentPage page;
        if (view is ContentPage cp)
        {
            cp.DataContext = viewModel;
            page = cp;
        }
        else
        {
            page = new ContentPage { Content = view, DataContext = viewModel };
        }

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
