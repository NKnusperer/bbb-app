using BlackBoxBuddy.Navigation;
using BlackBoxBuddy.ViewModels;
using BlackBoxBuddy.ViewModels.Shell;
using Microsoft.Extensions.DependencyInjection;

namespace BlackBoxBuddy;

public static class AppServices
{
    public static IServiceProvider ConfigureServices(
        Action<IServiceCollection>? platformServices = null)
    {
        var services = new ServiceCollection();

        // Platform-specific registrations first
        platformServices?.Invoke(services);

        // Shared singletons — device and service registrations added in Plan 02
        services.AddSingleton<INavigationService, NavigationService>();

        // Transient ViewModels
        services.AddTransient<AppShellViewModel>();
        services.AddTransient<DashboardViewModel>();
        services.AddTransient<RecordingsViewModel>();
        services.AddTransient<LiveFeedViewModel>();
        services.AddTransient<SettingsViewModel>();

        return services.BuildServiceProvider();
    }
}
