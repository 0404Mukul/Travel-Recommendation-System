namespace TravelRecommendationEngine.Models;

public sealed class User
{
    public int UserId { get; init; }
    public string Preferences { get; init; } = string.Empty;
    public string Gender { get; init; } = string.Empty;
    public int NumberOfAdults { get; init; }
    public int NumberOfChildren { get; init; }
}
