using TravelRecommendationEngine.Models;

namespace TravelRecommendationEngine.Services;

public interface IRecommendationService
{
    RecommendationResponse GetRecommendations(RecommendationRequest request);
}
