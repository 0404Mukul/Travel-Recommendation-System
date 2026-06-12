using TravelRecommendationEngine.Models;

namespace TravelRecommendationEngine.Services;

internal static class HotelRankingEngine
{
    private static readonly string[] FamilyAmenityKeywords = ["pool", "family", "breakfast", "garden", "spa"];

    public static IReadOnlyList<RecommendedHotel> RankHotels(
        IEnumerable<Hotel> hotels,
        RecommendationContext context,
        int maxRecommendations)
    {
        var normalizedPreferences = PreferenceNormalizer.ToSet(context.Preferences);
        var budget = context.MaxBudgetPerNight;

        return hotels
            .GroupBy(hotel => NormalizeHotelName(hotel.Name), StringComparer.OrdinalIgnoreCase)
            .Select(group => group.OrderByDescending(hotel => hotel.StarRating).ThenBy(hotel => hotel.PricePerNight).First())
            .Select(hotel =>
            {
                var (score, reasons) = ScoreHotel(hotel, normalizedPreferences, context, budget);
                return RecommendedHotel.FromHotel(hotel, score, reasons);
            })
            .Where(result => budget is null || result.PricePerNight <= budget * 1.15)
            .OrderByDescending(result => result.RecommendationScore)
            .ThenByDescending(result => result.StarRating)
            .ThenBy(result => result.PricePerNight)
            .Take(maxRecommendations)
            .ToList();
    }

    private static (double Score, List<string> Reasons) ScoreHotel(
        Hotel hotel,
        IReadOnlySet<string> preferences,
        RecommendationContext context,
        double? maxBudgetPerNight)
    {
        var reasons = new List<string>();
        var starScore = Math.Clamp(hotel.StarRating / 5.0, 0.0, 1.0);
        if (hotel.StarRating >= 4)
        {
            reasons.Add($"{hotel.StarRating}-star rated property.");
        }

        var affordabilityScore = ComputeAffordabilityScore(hotel.PricePerNight, maxBudgetPerNight);
        if (affordabilityScore >= 0.75)
        {
            reasons.Add("Fits comfortably within the expected nightly budget.");
        }
        else if (maxBudgetPerNight is not null && hotel.PricePerNight > maxBudgetPerNight)
        {
            reasons.Add("Slightly above budget but offers strong overall value.");
        }
        else
        {
            reasons.Add("Competitive price for the destination.");
        }

        var amenityScore = ComputeAmenityScore(hotel.Amenities, preferences);
        if (amenityScore >= 0.6)
        {
            reasons.Add("Amenities align with traveler preferences.");
        }

        var familyScore = context.IsFamilyTrip ? ComputeFamilyAmenityScore(hotel.Amenities) : 0.75;
        if (context.IsFamilyTrip && familyScore >= 0.7)
        {
            reasons.Add("Includes family-friendly amenities.");
        }

        var partySizeScore = ComputePartySizeScore(hotel.PricePerNight, context.PartySize);
        if (context.PartySize >= 4 && partySizeScore >= 0.7)
        {
            reasons.Add($"Suitable for a group of {context.PartySize} travelers.");
        }

        var preferenceHits = PreferenceNormalizer.CountMatches($"{hotel.Name} {hotel.Amenities}", preferences);
        if (preferenceHits > 0)
        {
            reasons.Add("Matches stated traveler preferences.");
        }

        var score = Math.Clamp(
            starScore * 0.30
            + affordabilityScore * 0.30
            + amenityScore * 0.20
            + familyScore * 0.10
            + partySizeScore * 0.10,
            0.0,
            1.0);

        if (reasons.Count == 0)
        {
            reasons.Add("Balanced option for this destination.");
        }

        return (score, reasons);
    }

    private static double ComputeAffordabilityScore(double pricePerNight, double? maxBudgetPerNight)
    {
        var budget = maxBudgetPerNight ?? 200.0;
        if (pricePerNight <= budget)
        {
            return 1.0 - Math.Clamp((pricePerNight - 50.0) / Math.Max(budget - 50.0, 1.0), 0.0, 0.35);
        }

        return Math.Clamp(1.0 - ((pricePerNight - budget) / budget), 0.2, 0.8);
    }

    private static double ComputeAmenityScore(string amenities, IReadOnlySet<string> preferences)
    {
        if (preferences.Count == 0)
        {
            return 0.6;
        }

        var hits = PreferenceNormalizer.CountMatches(amenities, preferences);
        return Math.Clamp(0.4 + hits * 0.2, 0.0, 1.0);
    }

    private static double ComputeFamilyAmenityScore(string amenities)
    {
        var normalized = amenities.ToLowerInvariant();
        var matches = FamilyAmenityKeywords.Count(keyword => normalized.Contains(keyword, StringComparison.Ordinal));
        return Math.Clamp(0.4 + matches * 0.15, 0.0, 1.0);
    }

    private static double ComputePartySizeScore(double pricePerNight, int partySize)
    {
        var estimatedPerPerson = pricePerNight / Math.Max(partySize, 1);
        return Math.Clamp(1.0 - (estimatedPerPerson - 25.0) / 120.0, 0.0, 1.0);
    }

    private static string NormalizeHotelName(string name) =>
        name.Trim().ToLowerInvariant();
}
