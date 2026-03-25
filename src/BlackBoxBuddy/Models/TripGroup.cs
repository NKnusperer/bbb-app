namespace BlackBoxBuddy.Models;

public record TripGroup(IReadOnlyList<Recording> Clips)
{
    public TimeSpan TotalDuration => Clips.Aggregate(TimeSpan.Zero, (sum, r) => sum + r.Duration);
    public double TotalDistance => Clips.Sum(r => r.Distance);
    public double AvgSpeed => TotalDuration.TotalSeconds > 0
        ? Clips.Sum(r => r.AvgSpeed * r.Duration.TotalSeconds) / TotalDuration.TotalSeconds
        : 0;
    public double PeakGForce => Clips.Max(r => r.PeakGForce);
    public DateTime StartTime => Clips.Min(r => r.DateTime);
    public DateTime EndTime => Clips.Max(r => r.DateTime + r.Duration);
}
