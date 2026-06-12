using TravelRecommendationEngine.Models;

namespace TravelRecommendationEngine.Services;

public interface IPromptBuilderService
{
    string BuildItineraryPrompt(string destinationName, string bestTimeToVisit, int days, bool isFamilyTrip);
    string BuildItineraryPrompt(
        string destinationName,
        string bestTimeToVisit,
        int days,
        bool isFamilyTrip,
        double? maxBudgetPerNight,
        WeatherForecastResponse weather,
        IReadOnlyList<RecommendedHotel> hotels,
        IReadOnlyList<RecommendedActivity> activities);
}
