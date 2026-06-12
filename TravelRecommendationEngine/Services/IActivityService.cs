using TravelRecommendationEngine.Models;

namespace TravelRecommendationEngine.Services;

public interface IActivityService
{
    IReadOnlyList<RecommendedActivity> GetRecommendedActivities(
        int destinationId,
        RecommendationContext context,
        int maxRecommendations = 3);
}
