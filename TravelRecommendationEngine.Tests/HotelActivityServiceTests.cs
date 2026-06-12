using TravelRecommendationEngine.Models;
using TravelRecommendationEngine.Services;

namespace TravelRecommendationEngine.Tests;

public sealed class HotelActivityServiceTests
{
    [Fact]
    public void GetRecommendedHotels_ReturnsScoredHotelsWithReasons()
    {
        var csvLoaderService = new InMemoryCsvLoaderService();
        var destinationService = new DestinationService(csvLoaderService);
        var hotelService = new HotelService(csvLoaderService, destinationService);
        var context = new RecommendationContext
        {
            User = new User { NumberOfAdults = 2, NumberOfChildren = 0 },
            Preferences = ["Beach"],
            MaxBudgetPerNight = 150
        };

        var hotels = hotelService.GetRecommendedHotels(1, context);

        Assert.NotEmpty(hotels);
        Assert.All(hotels, hotel => Assert.NotEmpty(hotel.Reasons));
        Assert.True(hotels[0].RecommendationScore >= hotels[^1].RecommendationScore);
    }

    [Fact]
    public void GetRecommendedHotels_DeduplicatesByName()
    {
        var csvLoaderService = new DuplicateHotelCsvLoader();
        var destinationService = new DestinationService(csvLoaderService);
        var hotelService = new HotelService(csvLoaderService, destinationService);
        var context = new RecommendationContext { User = new User { NumberOfAdults = 2 } };

        var hotels = hotelService.GetRecommendedHotels(10, context, 5);

        Assert.Single(hotels);
    }

    [Fact]
    public void GetRecommendedActivities_PrefersEasyActivitiesForFamilies()
    {
        var csvLoaderService = new FallbackCsvLoaderService();
        var destinationService = new DestinationService(csvLoaderService);
        var activityService = new ActivityService(csvLoaderService, destinationService);
        var context = new RecommendationContext
        {
            User = new User { NumberOfAdults = 2, NumberOfChildren = 2 },
            Preferences = ["Beach"]
        };

        var activities = activityService.GetRecommendedActivities(99, context);

        Assert.NotEmpty(activities);
        Assert.Equal("Easy", activities[0].Difficulty, ignoreCase: true);
        Assert.NotEmpty(activities[0].Reasons);
    }

    private sealed class InMemoryCsvLoaderService : ICsvLoaderService
    {
        public TravelData LoadTravelData() => CreateData();
        public TravelDataSnapshot GetSnapshot() => TravelDataSnapshot.Create(CreateData());

        private static TravelData CreateData() => new()
        {
            Destinations =
            [
                new Destination { DestinationId = 1, Name = "Goa Beaches", State = "Goa", Type = "Beach", Popularity = 9.0, BestTimeToVisit = "Nov-Mar" }
            ],
            Hotels =
            [
                new Hotel { HotelId = 1, DestinationId = 1, Name = "Seaview Resort", StarRating = 5, PricePerNight = 140, Amenities = "Pool, Spa, Beach Access" },
                new Hotel { HotelId = 2, DestinationId = 1, Name = "Beachside Inn", StarRating = 4, PricePerNight = 95, Amenities = "Breakfast, Sea View" }
            ],
            Activities =
            [
                new Activity { ActivityId = 1, DestinationId = 1, Name = "Sunset Cruise", Category = "Relaxation", DurationHours = 3, Difficulty = "Easy", Popularity = 9.0 }
            ]
        };
    }

    private sealed class DuplicateHotelCsvLoader : ICsvLoaderService
    {
        public TravelData LoadTravelData() => CreateData();
        public TravelDataSnapshot GetSnapshot() => TravelDataSnapshot.Create(CreateData());

        private static TravelData CreateData() => new()
        {
            Destinations =
            [
                new Destination { DestinationId = 10, Name = "Test City", State = "Test", Type = "City", Popularity = 8.0, BestTimeToVisit = "Year-round" }
            ],
            Hotels =
            [
                new Hotel { HotelId = 1, DestinationId = 10, Name = "Urban Grand", StarRating = 4, PricePerNight = 100, Amenities = "Wi-Fi" },
                new Hotel { HotelId = 2, DestinationId = 10, Name = "urban grand", StarRating = 3, PricePerNight = 80, Amenities = "Wi-Fi" }
            ]
        };
    }

    private sealed class FallbackCsvLoaderService : ICsvLoaderService
    {
        public TravelData LoadTravelData() => CreateData();
        public TravelDataSnapshot GetSnapshot() => TravelDataSnapshot.Create(CreateData());

        private static TravelData CreateData() => new()
        {
            Destinations =
            [
                new Destination
                {
                    DestinationId = 99,
                    Name = "Coastal Bay",
                    State = "Goa",
                    Type = "Beach",
                    Popularity = 8.5,
                    BestTimeToVisit = "Nov-Mar"
                }
            ]
        };
    }
}
