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
        // Transient ViewModels — added as ViewModels are created in Plans 02-04

        return services.BuildServiceProvider();
    }
}
