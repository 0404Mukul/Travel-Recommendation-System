using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using TravelRecommendationEngine.Configuration;
using TravelRecommendationEngine.Services;

namespace TravelRecommendationEngine.Tests;

internal static class TestServiceFactory
{
    public static RecommendationSettings Settings => new();

    public static IOptions<RecommendationSettings> Options => Microsoft.Extensions.Options.Options.Create(Settings);

    public static WeatherService CreateWeatherService(IDestinationService destinationService)
    {
        var configuration = new ConfigurationBuilder().Build();
        return new WeatherService(new HttpClient(), configuration, destinationService, Options);
    }

    public static RecommendationService CreateRecommendationService(
        ICsvLoaderService csvLoaderService,
        IWeatherService weatherService,
        IAnalyticsService? analyticsService = null)
    {
        var destinationService = new DestinationService(csvLoaderService);
        return new RecommendationService(
            csvLoaderService,
            destinationService,
            new UserPreferenceService(csvLoaderService),
            weatherService,
            new HotelService(csvLoaderService, destinationService),
            new ActivityService(csvLoaderService, destinationService),
            analyticsService ?? new AnalyticsService(csvLoaderService),
            Options);
    }
}
