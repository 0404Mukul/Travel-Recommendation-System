using System.ComponentModel.DataAnnotations;

namespace TravelRecommendationEngine.Models;

public sealed class RecommendationRequest
{
    [Range(1, int.MaxValue)]
    public int UserId { get; init; }

    [Range(20, 10000)]
    public double? MaxBudgetPerNight { get; init; }
}
