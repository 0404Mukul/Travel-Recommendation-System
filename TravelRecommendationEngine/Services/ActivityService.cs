using TravelRecommendationEngine.Models;

namespace TravelRecommendationEngine.Services;

public sealed class ActivityService : IActivityService
{
    private readonly ICsvLoaderService _csvLoaderService;
    private readonly IDestinationService _destinationService;

    public ActivityService(ICsvLoaderService csvLoaderService, IDestinationService destinationService)
    {
        _csvLoaderService = csvLoaderService;
        _destinationService = destinationService;
    }

    public IReadOnlyList<RecommendedActivity> GetRecommendedActivities(
        int destinationId,
        RecommendationContext context,
        int maxRecommendations = 3)
    {
        var snapshot = _csvLoaderService.GetSnapshot();
        var activities = snapshot.ActivitiesByDestinationId[destinationId].ToList();

        if (activities.Count == 0)
        {
            var destination = _destinationService.GetDestinationById(destinationId);
            if (destination is null)
            {
                return [];
            }

            activities = DestinationRecommendationsCatalog
                .CreateActivities(destination, maxRecommendations + 2, context.IsFamilyTrip)
                .ToList();
        }

        return ActivityRankingEngine.RankActivities(activities, context, maxRecommendations);
    }
}
