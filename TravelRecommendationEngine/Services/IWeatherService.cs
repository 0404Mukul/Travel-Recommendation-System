using TravelRecommendationEngine.Models;

namespace TravelRecommendationEngine.Services;

public interface IWeatherService
{
    WeatherForecastResponse GetWeather(string destinationName);
}
