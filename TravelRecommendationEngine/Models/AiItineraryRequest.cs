using System.ComponentModel.DataAnnotations;

namespace TravelRecommendationEngine.Models;

public sealed class AiItineraryRequest
{
    [Required]
    public string DestinationName { get; init; } = string.Empty;

    [Required]
    public string BestTimeToVisit { get; init; } = string.Empty;

    [Range(1, 30)]
    public int Days { get; init; } = 5;

    public bool IsFamilyTrip { get; init; } = true;

    [Range(1, 20)]
    public int PartySize { get; init; } = 2;

    [Range(20, 10000)]
    public double? MaxBudgetPerNight { get; init; }
}
