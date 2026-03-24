using BlackBoxBuddy.ViewModels;

namespace BlackBoxBuddy.Navigation;

public interface INavigationService
{
    Task PushAsync(ViewModelBase viewModel);
    Task PopAsync();
    Task PopToRootAsync();
}
