using System.Collections.Concurrent;
using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using TravelRecommendationEngine.Configuration;
using TravelRecommendationEngine.Models;

namespace TravelRecommendationEngine.Services;

public sealed class WeatherService : IWeatherService
{
    private readonly HttpClient _httpClient;
    private readonly string? _apiKey;
    private readonly IDestinationService _destinationService;
    private readonly RecommendationSettings _settings;
    private readonly ConcurrentDictionary<string, CachedWeather> _cache = new(StringComparer.OrdinalIgnoreCase);

    public WeatherService(
        HttpClient httpClient,
        IConfiguration configuration,
        IDestinationService destinationService,
        IOptions<RecommendationSettings> settings)
    {
        _httpClient = httpClient;
        _apiKey = configuration.GetValue<string>("OpenWeather:ApiKey");
        _destinationService = destinationService;
        _settings = settings.Value;
    }

    public WeatherForecastResponse GetWeather(string destinationName)
    {
        if (_cache.TryGetValue(destinationName, out var cached) && cached.ExpiresAt > DateTime.UtcNow)
        {
            return cached.Weather;
        }

        var destination = _destinationService.GetDestinationByName(destinationName);
        var baseWeather = FetchWeather(destinationName, destination);
        var enriched = WeatherRecommendationHelper.Enrich(baseWeather, destination, _settings);

        _cache[destinationName] = new CachedWeather(
            enriched,
            DateTime.UtcNow.AddMinutes(_settings.WeatherCacheMinutes));

        return enriched;
    }

    private WeatherForecastResponse FetchWeather(string destinationName, Destination? destination)
    {
        if (!string.IsNullOrWhiteSpace(_apiKey) && TryGetLocationQuery(destination, out var location))
        {
            try
            {
                var url = $"https://api.openweathermap.org/data/2.5/weather?q={Uri.EscapeDataString(location)},IN&appid={_apiKey}&units=metric";
                var weatherResponse = _httpClient.GetFromJsonAsync<OpenWeatherResponse>(url).GetAwaiter().GetResult();

                if (weatherResponse?.Main is not null)
                {
                    var forecastDescription = weatherResponse.Weather?.FirstOrDefault() is WeatherInfo info
                        ? info.Description
                        : "Clear skies";

                    return new WeatherForecastResponse
                    {
                        DestinationName = destinationName,
                        BestTimeToVisit = destination?.BestTimeToVisit ?? "Year-round",
                        Forecast = forecastDescription,
                        TemperatureCelsius = weatherResponse.Main.Temp
                    };
                }
            }
            catch
            {
                // Fallback to catalog weather on any HTTP or parsing error.
            }
        }

        if (destination is not null)
        {
            return DestinationRecommendationsCatalog.CreateWeatherFallback(destination);
        }

        return new WeatherForecastResponse
        {
            DestinationName = destinationName,
            BestTimeToVisit = "Year-round",
            Forecast = "Mild and suitable for travel",
            TemperatureCelsius = _settings.IdealTemperatureCelsius
        };
    }

    private static bool TryGetLocationQuery(Destination? destination, out string query)
    {
        if (destination is not null && !string.IsNullOrWhiteSpace(destination.State))
        {
            query = destination.State;
            return true;
        }

        if (destination is not null && !string.IsNullOrWhiteSpace(destination.Name))
        {
            query = destination.Name.Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? string.Empty;
            return query.Length > 0;
        }

        query = string.Empty;
        return false;
    }

    private sealed record CachedWeather(WeatherForecastResponse Weather, DateTime ExpiresAt);
    private sealed record OpenWeatherResponse(MainInfo Main, WeatherInfo[]? Weather);
    private sealed record MainInfo(double Temp);
    private sealed record WeatherInfo(string Description);
}
