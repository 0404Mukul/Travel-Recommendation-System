using Microsoft.Extensions.Configuration;
using TravelRecommendationEngine.Models;
using TravelRecommendationEngine.Services;

namespace TravelRecommendationEngine.Tests;

public sealed class WeatherServiceTests
{
    [Fact]
    public void GetWeather_ReturnsStructuredSummaryWhenOpenWeatherIsUnavailable()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["OpenWeather:ApiKey"] = string.Empty })
            .Build();

        var destinationService = new StubDestinationService(new Destination
        {
            DestinationId = 2,
            Name = "Goa Beaches",
            State = "Goa",
            Type = "Beach",
            Popularity = 9.0,
            BestTimeToVisit = "Nov-Mar"
        });

        var service = new WeatherService(new HttpClient(), configuration, destinationService, TestServiceFactory.Options);
        var weather = service.GetWeather("Goa Beaches");

        Assert.Equal("Goa Beaches", weather.DestinationName);
        Assert.True(weather.SuitabilityScore > 0);
        Assert.False(string.IsNullOrWhiteSpace(weather.RecommendationSummary));
        Assert.False(string.IsNullOrWhiteSpace(weather.TravelAdvice));
    }

    [Fact]
    public void GetWeather_UsesCacheForRepeatedRequests()
    {
        var configuration = new ConfigurationBuilder().Build();
        var destinationService = new StubDestinationService(new Destination
        {
            DestinationId = 3,
            Name = "Munnar Hills",
            State = "Kerala",
            Type = "Nature",
            Popularity = 8.0,
            BestTimeToVisit = "Sep-Mar"
        });

        var service = new WeatherService(new HttpClient(), configuration, destinationService, TestServiceFactory.Options);
        var first = service.GetWeather("Munnar Hills");
        var second = service.GetWeather("Munnar Hills");

        Assert.Equal(first.RecommendationSummary, second.RecommendationSummary);
    }

    private sealed class StubDestinationService : IDestinationService
    {
        private readonly Destination? _destination;

        public StubDestinationService(Destination? destination) => _destination = destination;

        public IReadOnlyList<Destination> GetDestinationsMatchingPreferences(IEnumerable<string> preferences) => [];
        public Destination? GetDestinationById(int destinationId) => _destination;
        public Destination? GetDestinationByName(string destinationName) =>
            _destination is not null && _destination.Name.Equals(destinationName, StringComparison.OrdinalIgnoreCase)
                ? _destination
                : null;
    }
}
