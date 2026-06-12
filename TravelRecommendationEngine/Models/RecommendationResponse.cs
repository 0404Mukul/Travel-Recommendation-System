namespace TravelRecommendationEngine.Models;

public sealed class RecommendationResponse
{
    public IReadOnlyList<RecommendationItem> Recommendations { get; init; } = [];
}
