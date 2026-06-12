namespace TravelRecommendationEngine.Models;

public sealed class AiItineraryResponse
{
    public string Prompt { get; init; } = string.Empty;
    public string Itinerary { get; init; } = string.Empty;
}
