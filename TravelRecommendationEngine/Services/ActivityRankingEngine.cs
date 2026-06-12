using TravelRecommendationEngine.Models;

namespace TravelRecommendationEngine.Services;

internal static class ActivityRankingEngine
{
    public static IReadOnlyList<RecommendedActivity> RankActivities(
        IEnumerable<Activity> activities,
        RecommendationContext context,
        int maxRecommendations)
    {
        var normalizedPreferences = PreferenceNormalizer.ToSet(context.Preferences);

        return activities
            .GroupBy(activity => NormalizeActivityName(activity.Name), StringComparer.OrdinalIgnoreCase)
            .Select(group => group.OrderByDescending(activity => activity.Popularity).First())
            .Select(activity =>
            {
                var (score, reasons) = ScoreActivity(activity, normalizedPreferences, context);
                return RecommendedActivity.FromActivity(activity, score, reasons);
            })
            .OrderByDescending(result => result.SuitabilityScore)
            .ThenByDescending(result => result.Popularity)
            .ThenBy(result => result.DurationHours)
            .Take(maxRecommendations)
            .ToList();
    }

    private static (double Score, List<string> Reasons) ScoreActivity(
        Activity activity,
        IReadOnlySet<string> preferences,
        RecommendationContext context)
    {
        var reasons = new List<string>();
        var popularityScore = Math.Clamp(activity.Popularity / 10.0, 0.0, 1.0);
        if (activity.Popularity >= 8.5)
        {
            reasons.Add("Highly popular experience at this destination.");
        }

        var difficultyScore = ScoreDifficulty(activity.Difficulty, context);
        if (context.IsFamilyTrip && difficultyScore >= 0.8)
        {
            reasons.Add("Family-friendly difficulty level.");
        }
        else if (!context.IsFamilyTrip && activity.Difficulty.Equals("Hard", StringComparison.OrdinalIgnoreCase))
        {
            reasons.Add("Great fit for adventurous travelers.");
        }

        var durationScore = ScoreDuration(activity.DurationHours, context);
        if (durationScore >= 0.75)
        {
            reasons.Add("Duration suits the travel party size.");
        }

        var preferenceScore = PreferenceNormalizer.CountMatches($"{activity.Name} {activity.Category}", preferences) > 0 ? 1.0 : 0.4;
        if (preferenceScore >= 1.0)
        {
            reasons.Add("Matches traveler activity preferences.");
        }

        var categoryFit = ScoreCategoryFit(activity.Category, context);
        if (categoryFit >= 0.8)
        {
            reasons.Add($"Strong {activity.Category.ToLowerInvariant()} experience for this trip.");
        }

        var score = Math.Clamp(
            popularityScore * 0.35
            + difficultyScore * 0.25
            + durationScore * 0.15
            + preferenceScore * 0.15
            + categoryFit * 0.10,
            0.0,
            1.0);

        if (reasons.Count == 0)
        {
            reasons.Add("Well-rounded activity for this destination.");
        }

        return (score, reasons);
    }

    private static double ScoreDifficulty(string difficulty, RecommendationContext context)
    {
        if (!context.IsFamilyTrip)
        {
            return difficulty.ToLowerInvariant() switch
            {
                "easy" => 0.75,
                "moderate" => 0.85,
                "hard" => 0.90,
                _ => 0.7
            };
        }

        return difficulty.Equals("Easy", StringComparison.OrdinalIgnoreCase) ? 1.0
            : difficulty.Equals("Moderate", StringComparison.OrdinalIgnoreCase) ? 0.65
            : 0.25;
    }

    private static double ScoreDuration(double durationHours, RecommendationContext context)
    {
        if (context.IsFamilyTrip)
        {
            return durationHours <= 3.0 ? 1.0 : durationHours <= 4.0 ? 0.75 : 0.45;
        }

        return durationHours is >= 2.0 and <= 5.0 ? 0.9 : 0.7;
    }

    private static double ScoreCategoryFit(string category, RecommendationContext context)
    {
        if (!context.IsFamilyTrip)
        {
            return 0.75;
        }

        var normalized = category.ToLowerInvariant();
        if (normalized is "relaxation" or "cultural" or "nature" or "wellness" or "leisure")
        {
            return 0.95;
        }

        return normalized is "adventure" ? 0.45 : 0.7;
    }

    private static string NormalizeActivityName(string name) =>
        name.Trim().ToLowerInvariant();
}
