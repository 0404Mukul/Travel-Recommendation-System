using Microsoft.AspNetCore.Mvc;
using TravelRecommendationEngine.Models;
using TravelRecommendationEngine.Services;

namespace TravelRecommendationEngine.Controllers;

[ApiController]
[Route("api/hotels")]
public sealed class HotelController : ControllerBase
{
    private readonly IHotelService _hotelService;

    public HotelController(IHotelService hotelService)
    {
        _hotelService = hotelService;
    }

    [HttpGet("{destinationId}")]
    public ActionResult<IEnumerable<RecommendedHotel>> GetHotels(
        int destinationId,
        [FromQuery] int maxRecommendations = 3,
        [FromQuery] double? maxBudgetPerNight = null,
        [FromQuery] int partySize = 2,
        [FromQuery] bool isFamilyTrip = false,
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
            MaxBudgetPerNight = maxBudgetPerNight
        };

        return Ok(_hotelService.GetRecommendedHotels(destinationId, context, maxRecommendations));
    }

    private static IReadOnlyList<string> ParsePreferences(string? preferences) =>
        string.IsNullOrWhiteSpace(preferences)
            ? []
            : preferences.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
}
