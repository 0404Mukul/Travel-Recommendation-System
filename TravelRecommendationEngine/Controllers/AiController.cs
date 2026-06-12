using Microsoft.AspNetCore.Mvc;
using TravelRecommendationEngine.Models;
using TravelRecommendationEngine.Services;

namespace TravelRecommendationEngine.Controllers;

[ApiController]
[Route("api/ai")]
public sealed class AiController : ControllerBase
{
    private readonly IItineraryBuilderService _itineraryBuilderService;

    public AiController(IItineraryBuilderService itineraryBuilderService)
    {
        _itineraryBuilderService = itineraryBuilderService;
    }

    [HttpPost("itinerary")]
    public ActionResult<AiItineraryResponse> GenerateItinerary([FromBody] AiItineraryRequest request)
    {
        return Ok(_itineraryBuilderService.BuildItinerary(request));
    }

    [HttpGet("demo-scenarios")]
    public ActionResult<IReadOnlyList<DemoScenario>> GetDemoScenarios()
    {
        return Ok(new List<DemoScenario>
        {
            new()
            {
                Name = "Beach family trip",
                Description = "Generate a family-friendly Goa itinerary with hotels and activities.",
                Endpoint = "/api/ai/itinerary",
                Method = "POST",
                SampleRequest = new { destinationName = "Goa Beaches", bestTimeToVisit = "Nov-Mar", days = 5, isFamilyTrip = true }
            },
            new()
            {
                Name = "Historical city break",
                Description = "Build a cultural itinerary for Jaipur with recommended stays.",
                Endpoint = "/api/ai/itinerary",
                Method = "POST",
                SampleRequest = new { destinationName = "Jaipur City", bestTimeToVisit = "Oct-Mar", days = 4, isFamilyTrip = false }
            },
            new()
            {
                Name = "Personalized recommendations",
                Description = "Get ranked destinations for a user with hotels, activities, and weather.",
                Endpoint = "/api/recommendations",
                Method = "POST",
                SampleRequest = new { userId = 1 }
            },
            new()
            {
                Name = "Weather check",
                Description = "Inspect live or fallback weather for a destination.",
                Endpoint = "/api/weather/Goa Beaches",
                Method = "GET",
                SampleRequest = new { }
            }
        });
    }
}
