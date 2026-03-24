using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using BlackBoxBuddy.Views;
using FluentAssertions;

namespace BlackBoxBuddy.Tests.Views;

public class SettingsPageTests
{
    [AvaloniaFact]
    public void SettingsPage_Instantiates_WithoutError()
    {
        var page = new SettingsPage();
        page.Should().NotBeNull();
    }

    [AvaloniaFact]
    public void SettingsPage_ContainsScrollViewer()
    {
        var page = new SettingsPage();
        var scrollViewer = page.FindControl<ScrollViewer>("SettingsScrollViewer");
        scrollViewer.Should().NotBeNull("SettingsPage should contain a ScrollViewer named SettingsScrollViewer");
    }
}
