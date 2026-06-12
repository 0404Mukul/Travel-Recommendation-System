using TravelRecommendationEngine.Configuration;
using TravelRecommendationEngine.Models;

namespace TravelRecommendationEngine.Services;

internal static class WeatherRecommendationHelper
{
    public static WeatherForecastResponse Enrich(
        WeatherForecastResponse weather,
        Destination? destination,
        RecommendationSettings settings)
    {
        var suitabilityScore = ComputeSuitabilityScore(weather, settings);
        var inSeason = SeasonHelper.IsInBestSeason(weather.BestTimeToVisit);
        var summary = BuildSummary(weather, suitabilityScore, inSeason);
        var advice = BuildTravelAdvice(weather, destination, inSeason, suitabilityScore);

        return new WeatherForecastResponse
        {
            DestinationName = weather.DestinationName,
            BestTimeToVisit = weather.BestTimeToVisit,
            Forecast = weather.Forecast,
            TemperatureCelsius = weather.TemperatureCelsius,
            SuitabilityScore = Math.Round(suitabilityScore, 2),
            RecommendationSummary = summary,
            TravelAdvice = advice,
            IsInBestSeason = inSeason
        };
    }

    public static double ComputeSuitabilityScore(WeatherForecastResponse weather, RecommendationSettings settings)
    {
        var forecast = weather.Forecast ?? string.Empty;
        var forecastBase = forecast.Contains("rain", StringComparison.OrdinalIgnoreCase)
                           || forecast.Contains("storm", StringComparison.OrdinalIgnoreCase) ? 0.40
            : forecast.Contains("snow", StringComparison.OrdinalIgnoreCase) ? 0.45
            : forecast.Contains("cloud", StringComparison.OrdinalIgnoreCase)
              || forecast.Contains("overcast", StringComparison.OrdinalIgnoreCase) ? 0.80
            : 1.00;

        var temperatureScore = 1.0 - Math.Clamp(
            Math.Abs(weather.TemperatureCelsius - settings.IdealTemperatureCelsius) / 20.0,
            0.0,
            1.0);

        var seasonMultiplier = SeasonHelper.IsInBestSeason(weather.BestTimeToVisit) ? 1.0 : 0.8;
        return Math.Clamp((forecastBase * 0.7 + temperatureScore * 0.3) * seasonMultiplier, 0.0, 1.0);
    }

    private static string BuildSummary(WeatherForecastResponse weather, double suitabilityScore, bool inSeason)
    {
        var comfort = suitabilityScore switch
        {
            >= 0.85 => "Excellent",
            >= 0.65 => "Good",
            >= 0.45 => "Fair",
            _ => "Challenging"
        };

        var seasonText = inSeason ? "within the best travel season" : "outside the peak travel season";
        return $"{comfort} travel weather ({weather.TemperatureCelsius:0.#}°C, {weather.Forecast}) — {seasonText}.";
    }

    private static string BuildTravelAdvice(
        WeatherForecastResponse weather,
        Destination? destination,
        bool inSeason,
        double suitabilityScore)
    {
        var advice = new List<string>();

        if (!inSeason)
        {
            advice.Add($"Consider planning around {weather.BestTimeToVisit} for optimal conditions.");
        }

        if (weather.TemperatureCelsius >= 30)
        {
            advice.Add("Pack light clothing, sunscreen, and stay hydrated.");
        }
        else if (weather.TemperatureCelsius <= 15)
        {
            advice.Add("Bring warm layers for cooler temperatures.");
        }

        if (weather.Forecast.Contains("rain", StringComparison.OrdinalIgnoreCase))
        {
            advice.Add("Carry rain gear and allow flexible indoor alternatives.");
        }

        if (destination?.Type.Equals("Adventure", StringComparison.OrdinalIgnoreCase) == true && suitabilityScore >= 0.7)
        {
            advice.Add("Weather is favorable for outdoor adventure activities.");
        }

        if (advice.Count == 0)
        {
            advice.Add("Conditions are generally comfortable for sightseeing and leisure.");
        }

        return string.Join(" ", advice);
    }
}
