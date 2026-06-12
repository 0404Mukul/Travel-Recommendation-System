namespace TravelRecommendationEngine.Models;

public sealed class Review
{
    public int ReviewId { get; init; }
    public int DestinationId { get; init; }
    public int UserId { get; init; }
    public double Rating { get; init; }
    public string ReviewText { get; init; } = string.Empty;
}
