using TravelRecommendationEngine.Models;
using TravelRecommendationEngine.Services;

namespace TravelRecommendationEngine.Tests;

public sealed class RankingEngineTests
{
    [Fact]
    public void HotelRankingEngine_PrefersHigherStarsWithinBudget()
    {
        var context = new RecommendationContext
        {
            User = new User { NumberOfAdults = 2 },
            Preferences = ["spa"],
            MaxBudgetPerNight = 150
        };

        var hotels = HotelRankingEngine.RankHotels(
        [
            new Hotel { HotelId = 1, DestinationId = 1, Name = "Budget Inn", StarRating = 3, PricePerNight = 80, Amenities = "Wi-Fi" },
            new Hotel { HotelId = 2, DestinationId = 1, Name = "Spa Resort", StarRating = 5, PricePerNight = 140, Amenities = "Pool, Spa" }
        ], context, 2);

        Assert.Equal("Spa Resort", hotels[0].Name);
        Assert.Contains(hotels[0].Reasons, reason => reason.Contains("star", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void ActivityRankingEngine_FamilyTripPrefersEasyActivities()
    {
        var context = new RecommendationContext
        {
            User = new User { NumberOfAdults = 2, NumberOfChildren = 2 },
            Preferences = ["cultural"]
        };

        var activities = ActivityRankingEngine.RankActivities(
        [
            new Activity { ActivityId = 1, DestinationId = 1, Name = "Rafting", Category = "Adventure", DurationHours = 3, Difficulty = "Hard", Popularity = 9.2 },
            new Activity { ActivityId = 2, DestinationId = 1, Name = "Museum Visit", Category = "Cultural", DurationHours = 2, Difficulty = "Easy", Popularity = 8.0 }
        ], context, 2);

        Assert.Equal("Museum Visit", activities[0].Name);
        Assert.True(activities[0].SuitabilityScore >= activities[1].SuitabilityScore);
    }
}
