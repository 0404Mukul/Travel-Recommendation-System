namespace TravelRecommendationEngine.Services;

internal static class PreferenceNormalizer
{
    public static HashSet<string> ToSet(IEnumerable<string> preferences) =>
        preferences
            .Select(Normalize)
            .Where(preference => preference.Length > 0)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

    public static int CountMatches(string searchableText, IReadOnlySet<string> preferences)
    {
        if (preferences.Count == 0)
        {
            return 0;
        }

        var normalizedText = searchableText.ToLowerInvariant();
        return preferences.Count(preference => normalizedText.Contains(preference, StringComparison.OrdinalIgnoreCase));
    }

    public static string Normalize(string value)
    {
        var normalized = value.Trim().ToLowerInvariant();
        if (normalized.EndsWith("ies", StringComparison.Ordinal))
        {
            return $"{normalized[..^3]}y";
        }

        if (normalized.EndsWith("ches", StringComparison.Ordinal)
            || normalized.EndsWith("shes", StringComparison.Ordinal)
            || normalized.EndsWith("xes", StringComparison.Ordinal)
            || normalized.EndsWith("zes", StringComparison.Ordinal)
            || normalized.EndsWith("ses", StringComparison.Ordinal))
        {
            return normalized[..^2];
        }

        return normalized.EndsWith('s') ? normalized[..^1] : normalized;
    }
}
