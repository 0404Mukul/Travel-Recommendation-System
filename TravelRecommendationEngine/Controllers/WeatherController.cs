using Microsoft.AspNetCore.Mvc;
using TravelRecommendationEngine.Models;
using TravelRecommendationEngine.Services;

namespace TravelRecommendationEngine.Controllers;

[ApiController]
[Route("api/weather")]
public sealed class WeatherController : ControllerBase
{
    private readonly IWeatherService _weatherService;

    public WeatherController(IWeatherService weatherService)
    {
        _weatherService = weatherService;
    }

    [HttpGet("{destinationName}")]
    public ActionResult<WeatherForecastResponse> GetWeather(string destinationName)
    {
        return Ok(_weatherService.GetWeather(destinationName));
    }
}
