using BlackBoxBuddy.Models;

namespace BlackBoxBuddy.Services;

public interface ITripGroupingService
{
    IReadOnlyList<object> Group(IReadOnlyList<Recording> recordings);
}
