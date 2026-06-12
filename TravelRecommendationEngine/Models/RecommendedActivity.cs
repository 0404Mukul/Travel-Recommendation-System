namespace TravelRecommendationEngine.Models;

public sealed class RecommendedActivity
{
    public int ActivityId { get; init; }
    public int DestinationId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Category { get; init; } = string.Empty;
    public double DurationHours { get; init; }
    public string Difficulty { get; init; } = string.Empty;
    public double Popularity { get; init; }
    public double SuitabilityScore { get; init; }
    public IReadOnlyList<string> Reasons { get; init; } = [];

    public static RecommendedActivity FromActivity(Activity activity, double suitabilityScore, IReadOnlyList<string> reasons) =>
        new()
        {
            ActivityId = activity.ActivityId,
            DestinationId = activity.DestinationId,
            Name = activity.Name,
            Category = activity.Category,
            DurationHours = activity.DurationHours,
            Difficulty = activity.Difficulty,
            Popularity = activity.Popularity,
            SuitabilityScore = Math.Round(suitabilityScore, 2),
            Reasons = reasons
        };
}
