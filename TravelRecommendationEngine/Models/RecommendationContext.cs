namespace TravelRecommendationEngine.Models;

public sealed class RecommendationContext
{
    public User User { get; init; } = new();
    public IReadOnlyList<string> Preferences { get; init; } = [];
    public double? MaxBudgetPerNight { get; init; }
    public int PartySize => User.NumberOfAdults + User.NumberOfChildren;
    public bool IsFamilyTrip => User.NumberOfChildren > 0;
}
