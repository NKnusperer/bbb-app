using Avalonia.Controls;
using Avalonia.Interactivity;
using BlackBoxBuddy.ViewModels;

namespace BlackBoxBuddy.Views;

public partial class RecordingDetailPage : ContentPage
{
    public RecordingDetailPage()
    {
        InitializeComponent();
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        if (DataContext is RecordingDetailViewModel vm)
            vm.InitializePlayback();

        // Wire up seek slider PointerReleased to avoid two-way binding feedback loop
        if (this.FindControl<Slider>("SeekSlider") is { } slider)
        {
            slider.AddHandler(PointerReleasedEvent, OnSeekSliderPointerReleased, handledEventsToo: false);
        }
    }

    private void OnUnloaded(object? sender, RoutedEventArgs e)
    {
        if (DataContext is IDisposable d)
            d.Dispose();
    }

    private void OnSeekSliderPointerReleased(object? sender, Avalonia.Input.PointerReleasedEventArgs e)
    {
        if (sender is Slider slider && DataContext is RecordingDetailViewModel vm)
        {
            vm.SeekToCommand.Execute((float)slider.Value);
        }
    }
}
