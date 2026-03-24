using BlackBoxBuddy.ViewModels;

namespace BlackBoxBuddy.Navigation;

public class NavigationService : INavigationService
{
    // NavigationPage reference will be set when the shell initializes
    // Full implementation wired in Plan 04 when NavigationPage is used for provisioning

    public Task PushAsync(ViewModelBase viewModel)
    {
        // TODO: Implement with Avalonia 12 NavigationPage in Plan 04
        return Task.CompletedTask;
    }

    public Task PopAsync()
    {
        return Task.CompletedTask;
    }

    public Task PopToRootAsync()
    {
        return Task.CompletedTask;
    }
}
