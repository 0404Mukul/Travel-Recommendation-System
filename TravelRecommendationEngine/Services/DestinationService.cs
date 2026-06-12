using TravelRecommendationEngine.Models;

namespace TravelRecommendationEngine.Services;

public sealed class DestinationService : IDestinationService
{
    private readonly ICsvLoaderService _csvLoaderService;

    public DestinationService(ICsvLoaderService csvLoaderService)
    {
        _csvLoaderService = csvLoaderService;
    }

    public IReadOnlyList<Destination> GetDestinationsMatchingPreferences(IEnumerable<string> preferences)
    {
        var normalizedPreferences = PreferenceNormalizer.ToSet(preferences);
        if (normalizedPreferences.Count == 0)
        {
            return [];
        }

        return _csvLoaderService.GetSnapshot().Data.Destinations
            .Where(destination => DestinationMatchesPreferences(destination, normalizedPreferences))
            .ToList();
    }

    public Destination? GetDestinationById(int destinationId)
    {
        return _csvLoaderService.GetSnapshot().BestDestinationById.GetValueOrDefault(destinationId);
    }

    public Destination? GetDestinationByName(string destinationName)
    {
        return _csvLoaderService.GetSnapshot().BestDestinationByName.GetValueOrDefault(destinationName);
    }

    private static bool DestinationMatchesPreferences(Destination destination, IReadOnlySet<string> normalizedPreferences)
    {
        var normalizedType = PreferenceNormalizer.Normalize(destination.Type);
        var normalizedName = PreferenceNormalizer.Normalize(destination.Name);

        return normalizedPreferences.Contains(normalizedType)
            || normalizedPreferences.Any(preference =>
                normalizedName.Contains(preference, StringComparison.OrdinalIgnoreCase)
                || preference.Contains(normalizedType, StringComparison.OrdinalIgnoreCase));
    }
}
