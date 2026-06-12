using Microsoft.AspNetCore.Mvc;
using TravelRecommendationEngine.Models;
using TravelRecommendationEngine.Services;

namespace TravelRecommendationEngine.Controllers;

[ApiController]
[Route("api/analytics")]
public sealed class AnalyticsController : ControllerBase
{
    private readonly IAnalyticsService _analyticsService;

    public AnalyticsController(IAnalyticsService analyticsService)
    {
        _analyticsService = analyticsService;
    }

    [HttpGet("summary")]
    public ActionResult<AnalyticsSummary> GetSummary([FromQuery] int recentHistoryLimit = 20)
    {
        return Ok(_analyticsService.GetSummary(recentHistoryLimit));
    }

    [HttpPost("selection")]
    public IActionResult TrackSelection([FromBody] DestinationSelectionRequest request)
    {
        _analyticsService.TrackDestinationSelection(request.UserId, request.DestinationName);
        return Ok(new { message = "Selection tracked." });
    }
}

public sealed class DestinationSelectionRequest
{
    public int UserId { get; init; }
    public string DestinationName { get; init; } = string.Empty;
}
