using Avalonia.Controls;
using Avalonia.Controls.Templates;
using BlackBoxBuddy.ViewModels;
using BlackBoxBuddy.ViewModels.Shell;
using BlackBoxBuddy.ViewModels.Provisioning;

namespace BlackBoxBuddy;

public class ViewLocator : IDataTemplate
{
    public Control? Build(object? param) => param switch
    {
        MainViewModel => new Views.MainView(),
        AppShellViewModel => new Views.Shell.AppShellView(),
        DashboardViewModel => new Views.DashboardPage(),
        RecordingsViewModel => new Views.RecordingsPage(),
        LiveFeedViewModel => new Views.LiveFeedPage(),
        SettingsViewModel => new Views.SettingsPage(),
        ManualConnectionViewModel => new Views.Shell.ManualConnectionDialog(),
        ProvisioningViewModel => new Views.Provisioning.ProvisioningPage(),
        _ => new TextBlock { Text = $"No view for {param?.GetType().Name}" }
    };

    public bool Match(object? data) => data is ViewModelBase;
}
