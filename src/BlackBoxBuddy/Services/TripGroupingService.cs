using BlackBoxBuddy.Models;

namespace BlackBoxBuddy.Services;

public class TripGroupingService : ITripGroupingService
{
    private const int GapThresholdSeconds = 30;

    public IReadOnlyList<object> Group(IReadOnlyList<Recording> recordings)
    {
        if (recordings.Count == 0)
            return [];

        // Sort newest-first (D-08)
        var sorted = recordings
            .OrderByDescending(r => r.DateTime)
            .ToList();

        var result = new List<object>();
        var currentGroup = new List<Recording> { sorted[0] };

        for (int i = 1; i < sorted.Count; i++)
        {
            // sorted[i] is OLDER than sorted[i-1]
            // The newer clip is currentGroup's last element (sorted[i-1])
            // The older clip is sorted[i]
            // Gap = start of newer - end of older
            var newerStart = currentGroup[^1].DateTime;
            var olderEnd = sorted[i].DateTime + sorted[i].Duration;
            var gap = newerStart - olderEnd;

            if (gap.TotalSeconds <= GapThresholdSeconds)
            {
                currentGroup.Add(sorted[i]);
            }
            else
            {
                EmitGroup(result, currentGroup);
                currentGroup = [sorted[i]];
            }
        }

        EmitGroup(result, currentGroup);
        return result;
    }

    private static void EmitGroup(List<object> result, List<Recording> group)
    {
        if (group.Count == 1)
            result.Add(group[0]);
        else
            result.Add(new TripGroup(group.AsReadOnly()));
    }
}
