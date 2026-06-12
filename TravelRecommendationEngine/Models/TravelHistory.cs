namespace TravelRecommendationEngine.Models;

public sealed class TravelHistory
{
    public int HistoryId { get; init; }
    public int UserId { get; init; }
    public int DestinationId { get; init; }
    public DateOnly VisitDate { get; init; }
    public double ExperienceRating { get; init; }
}
