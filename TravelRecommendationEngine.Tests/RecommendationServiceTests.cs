using TravelRecommendationEngine.Models;
using TravelRecommendationEngine.Services;

namespace TravelRecommendationEngine.Tests;

public sealed class RecommendationServiceTests
{
    [Fact]
    public void GetRecommendations_ReturnsTopFiveWithExplanationsAndBreakdown()
    {
        var csvLoaderService = new InMemoryCsvLoaderService();
        var recommendationService = TestServiceFactory.CreateRecommendationService(
            csvLoaderService,
            new InMemoryWeatherService());

        var response = recommendationService.GetRecommendations(new RecommendationRequest { UserId = 123, MaxBudgetPerNight = 180 });

        Assert.Equal(5, response.Recommendations.Count);
        Assert.Equal("Goa Beaches", response.Recommendations[0].DestinationName);
        Assert.True(response.Recommendations[0].RecommendationScore >= response.Recommendations[1].RecommendationScore);
        Assert.NotEmpty(response.Recommendations[0].RecommendedHotels);
        Assert.NotEmpty(response.Recommendations[0].RecommendedActivities);
        Assert.NotEmpty(response.Recommendations[0].Explanations);
        Assert.NotNull(response.Recommendations[0].ScoreBreakdown);
        Assert.NotEmpty(response.Recommendations[0].RecommendedHotels[0].Reasons);
    }

    [Fact]
    public void GetRecommendations_ThrowsWhenUserDoesNotExist()
    {
        var csvLoaderService = new InMemoryCsvLoaderService();
        var recommendationService = TestServiceFactory.CreateRecommendationService(
            csvLoaderService,
            new InMemoryWeatherService());

        Assert.Throws<KeyNotFoundException>(() =>
            recommendationService.GetRecommendations(new RecommendationRequest { UserId = 999 }));
    }

    [Fact]
    public void GetRecommendations_BudgetAwareHotelsPreferAffordableOptions()
    {
        var csvLoaderService = new InMemoryCsvLoaderService();
        var recommendationService = TestServiceFactory.CreateRecommendationService(
            csvLoaderService,
            new InMemoryWeatherService());

        var response = recommendationService.GetRecommendations(new RecommendationRequest
        {
            UserId = 123,
            MaxBudgetPerNight = 100
        });

        var goa = response.Recommendations.First(item => item.DestinationName == "Goa Beaches");
        Assert.All(goa.RecommendedHotels, hotel => Assert.True(hotel.PricePerNight <= 115));
    }

    private sealed class InMemoryCsvLoaderService : ICsvLoaderService
    {
        public TravelData LoadTravelData() => CreateData();

        public TravelDataSnapshot GetSnapshot() => TravelDataSnapshot.Create(CreateData());

        private static TravelData CreateData() => new()
        {
            Users =
            [
                new User
                {
                    UserId = 123,
                    Preferences = "Beaches, Historical",
                    Gender = "Female",
                    NumberOfAdults = 2,
                    NumberOfChildren = 2
                }
            ],
            Destinations =
            [
                new Destination { DestinationId = 1, Name = "Goa Beaches", State = "Goa", Type = "Beach", Popularity = 9.1, BestTimeToVisit = "Nov-Mar" },
                new Destination { DestinationId = 2, Name = "Jaipur Forts", State = "Rajasthan", Type = "Historical", Popularity = 8.7, BestTimeToVisit = "Oct-Mar" },
                new Destination { DestinationId = 3, Name = "Andaman Islands", State = "Andaman", Type = "Beach", Popularity = 8.9, BestTimeToVisit = "Nov-Apr" },
                new Destination { DestinationId = 4, Name = "Hampi Ruins", State = "Karnataka", Type = "Historical", Popularity = 8.2, BestTimeToVisit = "Oct-Feb" },
                new Destination { DestinationId = 5, Name = "Pondicherry Beach", State = "Puducherry", Type = "Beach", Popularity = 7.9, BestTimeToVisit = "Oct-Mar" },
                new Destination { DestinationId = 6, Name = "Mysore Palace", State = "Karnataka", Type = "Historical", Popularity = 7.8, BestTimeToVisit = "Oct-Feb" },
                new Destination { DestinationId = 7, Name = "Rishikesh Adventure", State = "Uttarakhand", Type = "Adventure", Popularity = 8.6, BestTimeToVisit = "Sep-Jun" }
            ],
            Reviews =
            [
                new Review { ReviewId = 1, DestinationId = 1, UserId = 123, Rating = 4.5, ReviewText = "Great" },
                new Review { ReviewId = 2, DestinationId = 1, UserId = 124, Rating = 4.7, ReviewText = "Lovely" },
                new Review { ReviewId = 3, DestinationId = 2, UserId = 123, Rating = 4.4, ReviewText = "Historic" },
                new Review { ReviewId = 4, DestinationId = 3, UserId = 123, Rating = 4.3, ReviewText = "Calm" },
                new Review { ReviewId = 5, DestinationId = 4, UserId = 123, Rating = 4.2, ReviewText = "Ancient" },
                new Review { ReviewId = 6, DestinationId = 5, UserId = 123, Rating = 4.1, ReviewText = "Nice" },
                new Review { ReviewId = 7, DestinationId = 6, UserId = 123, Rating = 4.0, ReviewText = "Good" }
            ],
            Hotels =
            [
                new Hotel { HotelId = 1, DestinationId = 1, Name = "Seaview Resort", StarRating = 5, PricePerNight = 140, Amenities = "Pool, Spa, Beach Access" },
                new Hotel { HotelId = 2, DestinationId = 1, Name = "Budget Beach Inn", StarRating = 3, PricePerNight = 90, Amenities = "Breakfast, Beach Access" },
                new Hotel { HotelId = 3, DestinationId = 2, Name = "Heritage Palace", StarRating = 5, PricePerNight = 160, Amenities = "Pool, Guided Tours" }
            ],
            Activities =
            [
                new Activity { ActivityId = 1, DestinationId = 1, Name = "Sunset Cruise", Category = "Relaxation", DurationHours = 3, Difficulty = "Easy", Popularity = 9.0 },
                new Activity { ActivityId = 2, DestinationId = 2, Name = "City Fort Tour", Category = "Cultural", DurationHours = 4, Difficulty = "Moderate", Popularity = 8.7 }
            ]
        };
    }

    private sealed class InMemoryWeatherService : IWeatherService
    {
        public WeatherForecastResponse GetWeather(string destinationName) =>
            new()
            {
                DestinationName = destinationName,
                BestTimeToVisit = "Nov-Mar",
                Forecast = "Sunny with pleasant sea breeze",
                TemperatureCelsius = 29,
                SuitabilityScore = 0.92,
                RecommendationSummary = "Excellent travel weather (29°C, sunny) — within the best travel season.",
                TravelAdvice = "Conditions are generally comfortable for sightseeing and leisure.",
                IsInBestSeason = true
            };
    }
}
