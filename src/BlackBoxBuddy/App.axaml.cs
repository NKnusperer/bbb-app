using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using BlackBoxBuddy.ViewModels.Shell;
using BlackBoxBuddy.Views;
using BlackBoxBuddy.Views.Shell;

namespace BlackBoxBuddy;

public class App : Application
{
    /// <summary>
    /// Platform-specific service registrations. Set by Desktop Program.cs or Android MainActivity.cs
    /// before the Avalonia app is built, per D-22.
    /// </summary>
    public static Action<IServiceCollection>? PlatformServices { get; set; }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
#if DEBUG
        this.AttachDeveloperTools();
#endif
    }

    public override void OnFrameworkInitializationCompleted()
    {
        // DI MUST initialize before any ViewModel is created (Pitfall 4)
        var provider = AppServices.ConfigureServices(PlatformServices);
        Ioc.Default.ConfigureServices(provider);

        var shellVm = Ioc.Default.GetRequiredService<AppShellViewModel>();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow { Content = new AppShellView { DataContext = shellVm } };
        }
        else if (ApplicationLifetime is IActivityApplicationLifetime activityLifetime)
        {
            activityLifetime.MainViewFactory = () => new AppShellView { DataContext = shellVm };
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            singleViewPlatform.MainView = new AppShellView { DataContext = shellVm };
        }

        base.OnFrameworkInitializationCompleted();

        // Trigger auto-discovery asynchronously after shell is displayed per CONN-01, D-12
        _ = shellVm.StartDiscoveryCommand.ExecuteAsync(null);
    }
}
