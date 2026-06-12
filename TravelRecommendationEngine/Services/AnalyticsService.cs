using System.Collections.Concurrent;
using TravelRecommendationEngine.Models;

namespace TravelRecommendationEngine.Services;

public sealed class AnalyticsService : IAnalyticsService
{
    private readonly ICsvLoaderService _csvLoaderService;
    private readonly ConcurrentQueue<RecommendationHistoryEntry> _history = new();
    private readonly ConcurrentDictionary<string, int> _recommendedCounts = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, int> _selectedCounts = new(StringComparer.OrdinalIgnoreCase);
    private int _totalRequests;

    public AnalyticsService(ICsvLoaderService csvLoaderService)
    {
        _csvLoaderService = csvLoaderService;
    }

    public void TrackRecommendationRequest(int userId, IReadOnlyList<RecommendationItem> recommendations, double? maxBudgetPerNight)
    {
        Interlocked.Increment(ref _totalRequests);

        _history.Enqueue(new RecommendationHistoryEntry
        {
            UserId = userId,
            RequestedAtUtc = DateTime.UtcNow,
            RecommendedDestinations = recommendations.Select(item => item.DestinationName).ToList(),
            MaxBudgetPerNight = maxBudgetPerNight
        });

        TrimHistory();

        foreach (var recommendation in recommendations)
        {
            _recommendedCounts.AddOrUpdate(recommendation.DestinationName, 1, (_, count) => count + 1);
        }
    }

    public void TrackDestinationSelection(int userId, string destinationName)
    {
        _selectedCounts.AddOrUpdate(destinationName, 1, (_, count) => count + 1);
    }

    public AnalyticsSummary GetSummary(int recentHistoryLimit = 20)
    {
        var preferenceStats = _csvLoaderService.GetSnapshot().Data.Users
            .SelectMany(user => user.Preferences.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            .GroupBy(preference => preference, StringComparer.OrdinalIgnoreCase)
            .Select(group => new PreferenceStat { Preference = group.Key, Count = group.Count() })
            .OrderByDescending(stat => stat.Count)
            .Take(10)
            .ToList();

        return new AnalyticsSummary
        {
            TotalRecommendationRequests = _totalRequests,
            MostRecommendedDestinations = ToDestinationStats(_recommendedCounts),
            MostSelectedDestinations = ToDestinationStats(_selectedCounts),
            PreferenceStatistics = preferenceStats,
            RecentHistory = _history.Reverse().Take(recentHistoryLimit).ToList()
        };
    }

    private static IReadOnlyList<DestinationStat> ToDestinationStats(ConcurrentDictionary<string, int> counts) =>
        counts.Select(pair => new DestinationStat { DestinationName = pair.Key, Count = pair.Value })
            .OrderByDescending(stat => stat.Count)
            .Take(10)
            .ToList();

    private void TrimHistory()
    {
        while (_history.Count > 200 && _history.TryDequeue(out _))
        {
        }
    }
}
