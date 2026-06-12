using Microsoft.AspNetCore.Mvc;
using TravelRecommendationEngine.Models;
using TravelRecommendationEngine.Services;

namespace TravelRecommendationEngine.Controllers;

[ApiController]
[Route("api/activities")]
public sealed class ActivityController : ControllerBase
{
    private readonly IActivityService _activityService;

    public ActivityController(IActivityService activityService)
    {
        _activityService = activityService;
    }

    [HttpGet("{destinationId}")]
    public ActionResult<IEnumerable<RecommendedActivity>> GetActivities(
        int destinationId,
        [FromQuery] int maxRecommendations = 3,
        [FromQuery] bool isFamilyTrip = false,
        [FromQuery] int partySize = 2,
        [FromQuery] string? preferences = null)
    {
        var children = isFamilyTrip ? Math.Max(1, partySize / 3) : 0;
        var context = new RecommendationContext
        {
            User = new User
            {
                NumberOfAdults = Math.Max(1, partySize - children),
                NumberOfChildren = children
            },
            Preferences = ParsePreferences(preferences),
        };

        return Ok(_activityService.GetRecommendedActivities(destinationId, context, maxRecommendations));
    }

    private static IReadOnlyList<string> ParsePreferences(string? preferences) =>
        string.IsNullOrWhiteSpace(preferences)
            ? []
            : preferences.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
}
