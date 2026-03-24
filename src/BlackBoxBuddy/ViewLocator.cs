using Avalonia.Controls;
using Avalonia.Controls.Templates;
using BlackBoxBuddy.ViewModels;

namespace BlackBoxBuddy;

public class ViewLocator : IDataTemplate
{
    public Control? Build(object? param) => param switch
    {
        MainViewModel => new Views.MainView(),
        // Future mappings added here as ViewModels are created in Plan 03/04
        _ => new TextBlock { Text = $"No view for {param?.GetType().Name}" }
    };

    public bool Match(object? data) => data is ViewModelBase;
}
