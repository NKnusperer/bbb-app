using BlackBoxBuddy.Models;

namespace BlackBoxBuddy.ViewModels;

/// <summary>
/// Minimal stub for RecordingDetailViewModel used by RecordingsViewModel navigation.
/// Full implementation in Plan 04.
/// </summary>
public class RecordingDetailViewModel : ViewModelBase
{
    public Recording? Recording { get; }
    public TripGroup? Trip { get; }
    public bool IsTripMode => Trip is not null;

    public RecordingDetailViewModel(Recording recording)
    {
        Recording = recording;
    }

    public RecordingDetailViewModel(TripGroup trip)
    {
        Trip = trip;
    }
}
