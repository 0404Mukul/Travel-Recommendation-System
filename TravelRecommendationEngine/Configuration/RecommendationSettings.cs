namespace TravelRecommendationEngine.Configuration;

public sealed class RecommendationSettings
{
    public const string SectionName = "Recommendation";

    public double PopularityWeight { get; init; } = 0.20;
    public double AverageRatingWeight { get; init; } = 0.17;
    public double WeatherWeight { get; init; } = 0.17;
    public double HotelWeight { get; init; } = 0.12;
    public double ActivityWeight { get; init; } = 0.12;
    public double PreferenceMatchWeight { get; init; } = 0.10;
    public double ReviewCountWeight { get; init; } = 0.07;
    public double BudgetFitWeight { get; init; } = 0.05;
    public double VisitedPenalty { get; init; } = -0.05;
    public int DefaultRecommendationCount { get; init; } = 5;
    public double IdealTemperatureCelsius { get; init; } = 24.0;
    public double DefaultMaxBudgetPerNight { get; init; } = 200.0;
    public int WeatherCacheMinutes { get; init; } = 30;
}
