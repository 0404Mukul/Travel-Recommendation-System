using TravelRecommendationEngine.Models;

namespace TravelRecommendationEngine.Services;

public interface IItineraryBuilderService
{
    AiItineraryResponse BuildItinerary(AiItineraryRequest request);
}
