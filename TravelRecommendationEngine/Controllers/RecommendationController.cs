using Microsoft.AspNetCore.Mvc;
using TravelRecommendationEngine.Models;
using TravelRecommendationEngine.Services;

namespace TravelRecommendationEngine.Controllers;

[ApiController]
[Route("api/recommendations")]
public sealed class RecommendationController : ControllerBase
{
    private readonly IRecommendationService _recommendationService;
    private readonly IPromptBuilderService _promptBuilderService;

    public RecommendationController(
        IRecommendationService recommendationService,
        IPromptBuilderService promptBuilderService)
    {
        _recommendationService = recommendationService;
        _promptBuilderService = promptBuilderService;
    }

    [HttpPost]
    public ActionResult<RecommendationResponse> GetRecommendations([FromBody] RecommendationRequest request)
    {
        try
        {
            return Ok(_recommendationService.GetRecommendations(request));
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(new { message = exception.Message });
        }
    }

    [HttpPost("itinerary-prompt")]
    public ActionResult<ItineraryPromptResponse> BuildItineraryPrompt([FromBody] ItineraryPromptRequest request)
    {
        var prompt = _promptBuilderService.BuildItineraryPrompt(
            request.DestinationName,
            request.BestTimeToVisit,
            request.Days,
            request.IsFamilyTrip);

        return Ok(new ItineraryPromptResponse { Prompt = prompt });
    }
}
