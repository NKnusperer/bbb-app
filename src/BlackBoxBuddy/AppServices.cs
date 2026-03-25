using BlackBoxBuddy.Device;
using BlackBoxBuddy.Device.Mock;
using BlackBoxBuddy.Navigation;
using BlackBoxBuddy.Services;
using BlackBoxBuddy.ViewModels;
using BlackBoxBuddy.ViewModels.Shell;
using BlackBoxBuddy.ViewModels.Provisioning;
using Microsoft.Extensions.DependencyInjection;
using Avalonia.Controls.ApplicationLifetimes;

namespace BlackBoxBuddy;

public static class AppServices
{
    public static IServiceProvider ConfigureServices(
        Action<IServiceCollection>? platformServices = null)
    {
        var services = new ServiceCollection();

        // Platform-specific registrations first
        platformServices?.Invoke(services);

        // Device layer — singleton mock device (swapped for real device in future phases)
        services.AddSingleton<IDashcamDevice, MockDashcamDevice>();

        // Services — singletons per D-23
        services.AddSingleton<IDeviceService, DeviceService>();
        services.AddSingleton<INavigationService, NavigationService>();
        services.AddSingleton<ITripGroupingService, TripGroupingService>();
        services.AddSingleton<IArchiveService, ArchiveService>();
        services.AddSingleton<IDialogService>(sp =>
        {
            return new DialogService(() =>
                Avalonia.Application.Current?.ApplicationLifetime is
                    IClassicDesktopStyleApplicationLifetime desktop
                        ? desktop.MainWindow : null);
        });

        // Transient ViewModels per D-23
        services.AddTransient<AppShellViewModel>();
        // Note: DashboardViewModel is NOT registered in DI.
        // It is constructed manually by AppShellViewModel with per-instance Action callbacks
        // (same pattern as ManualConnectionViewModel).
        services.AddTransient<RecordingsViewModel>();
        services.AddTransient<LiveFeedViewModel>();
        services.AddTransient<SettingsViewModel>();
        services.AddTransient<ProvisioningViewModel>();
        // Note: ManualConnectionViewModel is NOT registered in DI.
        // It is constructed manually by AppShellViewModel with a per-instance onClose callback.

        return services.BuildServiceProvider();
    }
}
