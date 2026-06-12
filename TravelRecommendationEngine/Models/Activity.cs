namespace TravelRecommendationEngine.Models;

public sealed class Activity
{
    public int ActivityId { get; init; }
    public int DestinationId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Category { get; init; } = string.Empty;
    public double DurationHours { get; init; }
    public string Difficulty { get; init; } = string.Empty;
    public double Popularity { get; init; }
}
