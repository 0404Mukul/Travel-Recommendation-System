using TravelRecommendationEngine.Models;

namespace TravelRecommendationEngine.Services;

public interface IHotelService
{
    IReadOnlyList<RecommendedHotel> GetRecommendedHotels(
        int destinationId,
        RecommendationContext context,
        int maxRecommendations = 3);
}
