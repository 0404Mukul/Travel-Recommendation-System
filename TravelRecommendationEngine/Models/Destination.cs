namespace TravelRecommendationEngine.Models;

public sealed class Destination
{
    public int DestinationId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string State { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;
    public double Popularity { get; init; }
    public string BestTimeToVisit { get; init; } = string.Empty;
}
