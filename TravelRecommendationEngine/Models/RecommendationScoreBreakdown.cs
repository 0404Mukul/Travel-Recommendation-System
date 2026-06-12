namespace TravelRecommendationEngine.Models;

public sealed class RecommendationScoreBreakdown
{
    public double Popularity { get; init; }
    public double AverageRating { get; init; }
    public double Weather { get; init; }
    public double Hotels { get; init; }
    public double Activities { get; init; }
    public double PreferenceMatch { get; init; }
    public double ReviewCount { get; init; }
    public double BudgetFit { get; init; }
    public double RevisitPenalty { get; init; }
}
