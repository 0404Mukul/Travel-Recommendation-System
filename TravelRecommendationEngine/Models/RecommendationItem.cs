namespace TravelRecommendationEngine.Models;

public sealed class RecommendationItem
{
    public int DestinationId { get; init; }
    public string DestinationName { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;
    public double Popularity { get; init; }
    public double AverageRating { get; init; }
    public double RecommendationScore { get; init; }
    public string BestTimeToVisit { get; init; } = string.Empty;
    public WeatherForecastResponse Weather { get; init; } = new();
    public IReadOnlyList<RecommendedHotel> RecommendedHotels { get; init; } = [];
    public IReadOnlyList<RecommendedActivity> RecommendedActivities { get; init; } = [];
    public IReadOnlyList<string> Explanations { get; init; } = [];
    public RecommendationScoreBreakdown? ScoreBreakdown { get; init; }
}
