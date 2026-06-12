namespace TravelRecommendationEngine.Models;

public sealed class AnalyticsSummary
{
    public int TotalRecommendationRequests { get; init; }
    public IReadOnlyList<DestinationStat> MostRecommendedDestinations { get; init; } = [];
    public IReadOnlyList<DestinationStat> MostSelectedDestinations { get; init; } = [];
    public IReadOnlyList<PreferenceStat> PreferenceStatistics { get; init; } = [];
    public IReadOnlyList<RecommendationHistoryEntry> RecentHistory { get; init; } = [];
}

public sealed class DestinationStat
{
    public string DestinationName { get; init; } = string.Empty;
    public int Count { get; init; }
}

public sealed class PreferenceStat
{
    public string Preference { get; init; } = string.Empty;
    public int Count { get; init; }
}

public sealed class RecommendationHistoryEntry
{
    public int UserId { get; init; }
    public DateTime RequestedAtUtc { get; init; }
    public IReadOnlyList<string> RecommendedDestinations { get; init; } = [];
    public double? MaxBudgetPerNight { get; init; }
}
