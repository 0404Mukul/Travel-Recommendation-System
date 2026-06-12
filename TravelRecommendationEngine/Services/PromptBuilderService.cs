using System.Text;
using TravelRecommendationEngine.Models;

namespace TravelRecommendationEngine.Services;

public sealed class PromptBuilderService : IPromptBuilderService
{
    public string BuildItineraryPrompt(string destinationName, string bestTimeToVisit, int days, bool isFamilyTrip)
    {
        var tripType = isFamilyTrip ? "family" : "traveler";
        return $"Create a {days}-day itinerary for {destinationName} during {bestTimeToVisit} for a {tripType} trip.";
    }

    public string BuildItineraryPrompt(
        string destinationName,
        string bestTimeToVisit,
        int days,
        bool isFamilyTrip,
        double? maxBudgetPerNight,
        WeatherForecastResponse weather,
        IReadOnlyList<RecommendedHotel> hotels,
        IReadOnlyList<RecommendedActivity> activities)
    {
        var builder = new StringBuilder();
        var tripType = isFamilyTrip ? "family" : "traveler";

        builder.AppendLine($"Create a {days}-day itinerary for {destinationName} during {bestTimeToVisit} for a {tripType} trip.");
        if (maxBudgetPerNight is not null)
        {
            builder.AppendLine($"Nightly accommodation budget: ₹{maxBudgetPerNight:0}.");
        }

        builder.AppendLine($"Weather context: {weather.RecommendationSummary}");
        builder.AppendLine($"Travel advice: {weather.TravelAdvice}");

        if (hotels.Count > 0)
        {
            builder.AppendLine("Preferred hotels:");
            foreach (var hotel in hotels)
            {
                builder.AppendLine($"- {hotel.Name} ({hotel.StarRating} stars, ₹{hotel.PricePerNight:0}/night) — {string.Join(", ", hotel.Reasons)}");
            }
        }

        if (activities.Count > 0)
        {
            builder.AppendLine("Preferred activities:");
            foreach (var activity in activities)
            {
                builder.AppendLine($"- {activity.Name} ({activity.Category}, {activity.Difficulty}) — {string.Join(", ", activity.Reasons)}");
            }
        }

        builder.Append("Include daily plans, meal suggestions, budget-conscious options, and travel tips suitable for the trip type.");

        return builder.ToString().TrimEnd();
    }
}
