using Avalonia.Controls;

namespace BlackBoxBuddy.Views;

public partial class LiveFeedPage : ContentPage
{
    protected override Type StyleKeyOverride => typeof(ContentPage);

    public LiveFeedPage() => InitializeComponent();
}
