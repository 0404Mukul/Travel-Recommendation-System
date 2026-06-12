using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using TravelRecommendationEngine.Controllers;
using TravelRecommendationEngine.Models;
using TravelRecommendationEngine.Services;

namespace TravelRecommendationEngine.Tests;

public sealed class ControllerTests
{
    [Fact]
    public void HotelController_ReturnsRecommendedHotelsWithReasons()
    {
        var csvLoaderService = new DemoCsvLoader();
        var destinationService = new DestinationService(csvLoaderService);
        var controller = new HotelController(new HotelService(csvLoaderService, destinationService));

        var result = controller.GetHotels(2, maxBudgetPerNight: 200);
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var hotels = Assert.IsAssignableFrom<IEnumerable<RecommendedHotel>>(okResult.Value);

        Assert.NotEmpty(hotels);
        Assert.All(hotels, hotel => Assert.NotEmpty(hotel.Reasons));
    }

    [Fact]
    public void ActivityController_ReturnsRecommendedActivitiesWithSuitability()
    {
        var csvLoaderService = new DemoCsvLoader();
        var destinationService = new DestinationService(csvLoaderService);
        var controller = new ActivityController(new ActivityService(csvLoaderService, destinationService));

        var result = controller.GetActivities(2, isFamilyTrip: true);
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var activities = Assert.IsAssignableFrom<IEnumerable<RecommendedActivity>>(okResult.Value);

        Assert.NotEmpty(activities);
        Assert.True(activities.First().SuitabilityScore > 0);
    }

    [Fact]
    public void AiController_GeneratesDataDrivenItineraryResponse()
    {
        var csvLoaderService = new DemoCsvLoader();
        var destinationService = new DestinationService(csvLoaderService);
        var analytics = new AnalyticsService(csvLoaderService);
        var itineraryBuilder = new ItineraryBuilderService(
            destinationService,
            new HotelService(csvLoaderService, destinationService),
            new ActivityService(csvLoaderService, destinationService),
            TestServiceFactory.CreateWeatherService(destinationService),
            new PromptBuilderService(),
            analytics);
        var controller = new AiController(itineraryBuilder);

        var request = new AiItineraryRequest
        {
            DestinationName = "Goa Beaches",
            BestTimeToVisit = "Nov-Mar",
            Days = 4,
            IsFamilyTrip = true,
            PartySize = 4,
            MaxBudgetPerNight = 150
        };

        var result = controller.GenerateItinerary(request);
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<AiItineraryResponse>(okResult.Value);

        Assert.Contains("Goa Beaches", response.Prompt);
        Assert.Contains("Day-by-day plan", response.Itinerary);
        Assert.Contains("Travel tips", response.Itinerary);
    }

    private sealed class DemoCsvLoader : ICsvLoaderService
    {
        public TravelData LoadTravelData() => CreateData();
        public TravelDataSnapshot GetSnapshot() => TravelDataSnapshot.Create(CreateData());

        private static TravelData CreateData() => new()
        {
            Destinations =
            [
                new Destination { DestinationId = 2, Name = "Goa Beaches", State = "Goa", Type = "Beach", Popularity = 9.0, BestTimeToVisit = "Nov-Mar" }
            ],
            Hotels =
            [
                new Hotel { HotelId = 1, DestinationId = 2, Name = "Seaview Resort", StarRating = 5, PricePerNight = 140, Amenities = "Pool;Spa;Beach Access" }
            ],
            Activities =
            [
                new Activity { ActivityId = 1, DestinationId = 2, Name = "Sunset Cruise", Category = "Relaxation", DurationHours = 3, Difficulty = "Easy", Popularity = 9.0 }
            ]
        };
    }
}
