using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using BlackBoxBuddy.ViewModels;
using BlackBoxBuddy.Views;

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

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow { DataContext = new MainViewModel() };
        }
        else if (ApplicationLifetime is IActivityApplicationLifetime activityLifetime)
        {
            activityLifetime.MainViewFactory = () => new MainView { DataContext = new MainViewModel() };
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            singleViewPlatform.MainView = new MainView { DataContext = new MainViewModel() };
        }

        base.OnFrameworkInitializationCompleted();
    }
}
