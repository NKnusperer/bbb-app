using Avalonia.Controls;
using BlackBoxBuddy.ViewModels;

namespace BlackBoxBuddy.Navigation;

public interface INavigationService
{
    void SetNavigationPage(NavigationPage page);
    Task PushAsync(ViewModelBase viewModel);
    Task PopAsync();
    Task PopToRootAsync();
}
