using Android.Content.PM;
using Avalonia;
using Avalonia.Android;

namespace BlackBoxBuddy.Android;

[Activity(
    Label = "BlackBoxBuddy.Android",
    Theme = "@style/MyTheme.NoActionBar",
    Icon = "@drawable/icon",
    MainLauncher = true,
    ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.UiMode)]
public class MainActivity : AvaloniaMainActivity
{
    protected override AppBuilder CustomizeAppBuilder(AppBuilder builder)
    {
        // Android-specific service registrations per D-22
        App.PlatformServices = services =>
        {
            // Android-specific services added here in future phases
            // e.g., ExoPlayer video playback in Phase 4
        };

        return base.CustomizeAppBuilder(builder);
    }
}
