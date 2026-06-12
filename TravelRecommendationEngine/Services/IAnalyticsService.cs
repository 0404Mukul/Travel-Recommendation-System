using TravelRecommendationEngine.Models;

namespace TravelRecommendationEngine.Services;

public interface IAnalyticsService
{
    void TrackRecommendationRequest(int userId, IReadOnlyList<RecommendationItem> recommendations, double? maxBudgetPerNight);
    void TrackDestinationSelection(int userId, string destinationName);
    AnalyticsSummary GetSummary(int recentHistoryLimit = 20);
}
