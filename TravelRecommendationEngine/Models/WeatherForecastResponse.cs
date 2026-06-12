namespace TravelRecommendationEngine.Models;

public sealed class WeatherForecastResponse
{
    public string DestinationName { get; init; } = string.Empty;
    public string BestTimeToVisit { get; init; } = string.Empty;
    public string Forecast { get; init; } = string.Empty;
    public double TemperatureCelsius { get; init; }
    public double SuitabilityScore { get; init; }
    public string RecommendationSummary { get; init; } = string.Empty;
    public string TravelAdvice { get; init; } = string.Empty;
    public bool IsInBestSeason { get; init; }
}
