using TravelRecommendationEngine.Models;
using TravelRecommendationEngine.Services;

namespace TravelRecommendationEngine.Tests;

public sealed class AnalyticsServiceTests
{
    [Fact]
    public void TrackRecommendationRequest_UpdatesSummary()
    {
        var analytics = new AnalyticsService(new EmptyCsvLoader());
        var recommendations = new List<RecommendationItem>
        {
            new() { DestinationName = "Goa Beaches", RecommendationScore = 9.1 },
            new() { DestinationName = "Jaipur City", RecommendationScore = 8.7 }
        };

        analytics.TrackRecommendationRequest(1, recommendations, 150);
        analytics.TrackDestinationSelection(1, "Goa Beaches");

        var summary = analytics.GetSummary();

        Assert.Equal(1, summary.TotalRecommendationRequests);
        Assert.Contains(summary.MostRecommendedDestinations, stat => stat.DestinationName == "Goa Beaches");
        Assert.Contains(summary.MostSelectedDestinations, stat => stat.DestinationName == "Goa Beaches");
        Assert.Single(summary.RecentHistory);
    }

    private sealed class EmptyCsvLoader : ICsvLoaderService
    {
        public TravelData LoadTravelData() => new()
        {
            Users =
            [
                new User { UserId = 1, Preferences = "Beaches, Historical" }
            ]
        };

        public TravelDataSnapshot GetSnapshot() => TravelDataSnapshot.Create(LoadTravelData());
    }
}
