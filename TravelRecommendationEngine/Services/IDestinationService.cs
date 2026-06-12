using TravelRecommendationEngine.Models;

namespace TravelRecommendationEngine.Services;

public interface IDestinationService
{
    IReadOnlyList<Destination> GetDestinationsMatchingPreferences(IEnumerable<string> preferences);
    Destination? GetDestinationById(int destinationId);
    Destination? GetDestinationByName(string destinationName);
}
