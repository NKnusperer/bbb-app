using Avalonia;
using Avalonia.Headless;
using Avalonia.Markup.Xaml;
using Avalonia.Themes.Fluent;
using BlackBoxBuddy.Tests;

[assembly: AvaloniaTestApplication(typeof(TestApp))]

namespace BlackBoxBuddy.Tests;

public class TestApp : Application
{
    public override void Initialize()
    {
        Styles.Add(new FluentTheme());
    }
}
