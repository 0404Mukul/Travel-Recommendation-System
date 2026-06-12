using TravelRecommendationEngine.Configuration;
using TravelRecommendationEngine.Models;

namespace TravelRecommendationEngine.Services;

internal readonly record struct DestinationScoreResult(
    double RawScore,
    RecommendationScoreBreakdown Breakdown,
    IReadOnlyList<string> Explanations);

internal static class RecommendationScoringEngine
{
    public static DestinationScoreResult ScoreDestination(
        Destination destination,
        ReviewStats? reviewStats,
        WeatherForecastResponse weather,
        IReadOnlyList<RecommendedHotel> hotels,
        IReadOnlyList<RecommendedActivity> activities,
        IReadOnlySet<string> normalizedPreferences,
        bool visitedBefore,
        RecommendationContext context,
        RecommendationSettings settings)
    {
        var popularityScore = Math.Clamp(destination.Popularity / 10.0, 0.0, 1.0);
        var averageRating = reviewStats?.AverageRating ?? 0.0;
        var reviewCount = reviewStats?.Count ?? 0;
        var ratingScore = Math.Clamp(averageRating / 5.0, 0.0, 1.0);
        var weatherScore = weather.SuitabilityScore > 0
            ? Math.Clamp(weather.SuitabilityScore, 0.0, 1.0)
            : WeatherRecommendationHelper.ComputeSuitabilityScore(weather, settings);
        var hotelScore = hotels.Count == 0 ? 0.0 : hotels.Average(hotel => hotel.RecommendationScore);
        var activityScore = activities.Count == 0 ? 0.0 : activities.Average(activity => activity.SuitabilityScore);
        var preferenceMatchScore = ComputePreferenceMatchScore(normalizedPreferences, destination, hotels, activities);
        var reviewCountScore = Math.Clamp(Math.Log10(reviewCount + 1.0) / 1.2, 0.0, 1.0);
        var budgetFitScore = ComputeBudgetFitScore(hotels, context.MaxBudgetPerNight ?? settings.DefaultMaxBudgetPerNight);
        var revisitPenalty = visitedBefore ? settings.VisitedPenalty : 0.0;

        var rawScore = settings.PopularityWeight * popularityScore
            + settings.AverageRatingWeight * ratingScore
            + settings.WeatherWeight * weatherScore
            + settings.HotelWeight * hotelScore
            + settings.ActivityWeight * activityScore
            + settings.PreferenceMatchWeight * preferenceMatchScore
            + settings.ReviewCountWeight * reviewCountScore
            + settings.BudgetFitWeight * budgetFitScore
            + revisitPenalty;

        var breakdown = new RecommendationScoreBreakdown
        {
            Popularity = Math.Round(popularityScore, 2),
            AverageRating = Math.Round(ratingScore, 2),
            Weather = Math.Round(weatherScore, 2),
            Hotels = Math.Round(hotelScore, 2),
            Activities = Math.Round(activityScore, 2),
            PreferenceMatch = Math.Round(preferenceMatchScore, 2),
            ReviewCount = Math.Round(reviewCountScore, 2),
            BudgetFit = Math.Round(budgetFitScore, 2),
            RevisitPenalty = revisitPenalty
        };

        var explanations = BuildExplanations(
            destination,
            averageRating,
            reviewCount,
            weather,
            hotels,
            activities,
            preferenceMatchScore,
            budgetFitScore,
            visitedBefore,
            context);

        return new DestinationScoreResult(Math.Clamp(rawScore, -0.2, 1.0), breakdown, explanations);
    }

    private static double ComputePreferenceMatchScore(
        IReadOnlySet<string> normalizedPreferences,
        Destination destination,
        IReadOnlyList<RecommendedHotel> hotels,
        IReadOnlyList<RecommendedActivity> activities)
    {
        if (normalizedPreferences.Count == 0)
        {
            return 0.5;
        }

        var normalizedType = PreferenceNormalizer.Normalize(destination.Type);
        var normalizedName = PreferenceNormalizer.Normalize(destination.Name);
        var typeMatch = normalizedPreferences.Contains(normalizedType)
            || normalizedPreferences.Any(preference => normalizedName.Contains(preference, StringComparison.OrdinalIgnoreCase))
            ? 1.0
            : 0.0;
        var hotelMatch = hotels.Any(hotel => hotel.Reasons.Any(reason => reason.Contains("preference", StringComparison.OrdinalIgnoreCase))) ? 1.0 : 0.0;
        var activityMatch = activities.Any(activity => activity.Reasons.Any(reason => reason.Contains("preference", StringComparison.OrdinalIgnoreCase))) ? 1.0 : 0.0;

        return Math.Clamp(typeMatch * 0.45 + hotelMatch * 0.30 + activityMatch * 0.25, 0.0, 1.0);
    }

    private static double ComputeBudgetFitScore(IReadOnlyList<RecommendedHotel> hotels, double budget)
    {
        if (hotels.Count == 0)
        {
            return 0.5;
        }

        var affordableCount = hotels.Count(hotel => hotel.PricePerNight <= budget);
        var averageAffordability = hotels.Average(hotel =>
            Math.Clamp(1.0 - Math.Max(0.0, hotel.PricePerNight - budget) / budget, 0.0, 1.0));

        return Math.Clamp(affordableCount / (double)hotels.Count * 0.5 + averageAffordability * 0.5, 0.0, 1.0);
    }

    private static List<string> BuildExplanations(
        Destination destination,
        double averageRating,
        int reviewCount,
        WeatherForecastResponse weather,
        IReadOnlyList<RecommendedHotel> hotels,
        IReadOnlyList<RecommendedActivity> activities,
        double preferenceMatchScore,
        double budgetFitScore,
        bool visitedBefore,
        RecommendationContext context)
    {
        var explanations = new List<string>
        {
            $"Matches your interest in {destination.Type.ToLowerInvariant()} destinations."
        };

        if (destination.Popularity >= 8.5)
        {
            explanations.Add($"Highly popular destination (score {destination.Popularity:0.0}/10).");
        }

        if (averageRating >= 4.0 && reviewCount > 0)
        {
            explanations.Add($"Strong traveler reviews ({averageRating:0.0}/5 from {reviewCount} reviews).");
        }

        if (!string.IsNullOrWhiteSpace(weather.RecommendationSummary))
        {
            explanations.Add(weather.RecommendationSummary);
        }

        if (preferenceMatchScore >= 0.8)
        {
            explanations.Add("Strong alignment with your stated preferences.");
        }

        if (budgetFitScore >= 0.75 && context.MaxBudgetPerNight is not null)
        {
            explanations.Add($"Hotel options fit your nightly budget of ₹{context.MaxBudgetPerNight:0}.");
        }

        if (context.IsFamilyTrip && activities.Any(activity => activity.Difficulty.Equals("Easy", StringComparison.OrdinalIgnoreCase)))
        {
            explanations.Add("Includes family-friendly activities.");
        }

        if (hotels.Count > 0)
        {
            explanations.Add($"Top stay suggestion: {hotels[0].Name} ({hotels[0].StarRating}★).");
        }

        if (visitedBefore)
        {
            explanations.Add("You have visited this destination before — ranked slightly lower to surface new options.");
        }

        return explanations;
    }
}
