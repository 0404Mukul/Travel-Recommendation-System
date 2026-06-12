using System.Text;
using TravelRecommendationEngine.Models;

namespace TravelRecommendationEngine.Services;

public sealed class ItineraryBuilderService : IItineraryBuilderService
{
    private readonly IDestinationService _destinationService;
    private readonly IHotelService _hotelService;
    private readonly IActivityService _activityService;
    private readonly IWeatherService _weatherService;
    private readonly IPromptBuilderService _promptBuilderService;
    private readonly IAnalyticsService _analyticsService;

    public ItineraryBuilderService(
        IDestinationService destinationService,
        IHotelService hotelService,
        IActivityService activityService,
        IWeatherService weatherService,
        IPromptBuilderService promptBuilderService,
        IAnalyticsService analyticsService)
    {
        _destinationService = destinationService;
        _hotelService = hotelService;
        _activityService = activityService;
        _weatherService = weatherService;
        _promptBuilderService = promptBuilderService;
        _analyticsService = analyticsService;
    }

    public AiItineraryResponse BuildItinerary(AiItineraryRequest request)
    {
        var destination = _destinationService.GetDestinationByName(request.DestinationName);
        var weather = _weatherService.GetWeather(request.DestinationName);
        var context = BuildContext(request);
        var hotels = destination is not null
            ? _hotelService.GetRecommendedHotels(destination.DestinationId, context, 3)
            : [];
        var activities = destination is not null
            ? _activityService.GetRecommendedActivities(destination.DestinationId, context, 4)
            : [];

        var prompt = _promptBuilderService.BuildItineraryPrompt(
            request.DestinationName,
            request.BestTimeToVisit,
            request.Days,
            request.IsFamilyTrip,
            request.MaxBudgetPerNight,
            weather,
            hotels,
            activities);

        var itinerary = BuildItineraryText(request, weather, hotels, activities, destination?.Type);

        _analyticsService.TrackDestinationSelection(0, request.DestinationName);

        return new AiItineraryResponse
        {
            Prompt = prompt,
            Itinerary = itinerary
        };
    }

    private static RecommendationContext BuildContext(AiItineraryRequest request)
    {
        var children = request.IsFamilyTrip ? Math.Max(1, request.PartySize / 3) : 0;
        var adults = Math.Max(1, request.PartySize - children);

        return new RecommendationContext
        {
            User = new User
            {
                NumberOfAdults = adults,
                NumberOfChildren = children
            },
            Preferences = [],
            MaxBudgetPerNight = request.MaxBudgetPerNight
        };
    }

    private static string BuildItineraryText(
        AiItineraryRequest request,
        WeatherForecastResponse weather,
        IReadOnlyList<RecommendedHotel> hotels,
        IReadOnlyList<RecommendedActivity> activities,
        string? destinationType)
    {
        var builder = new StringBuilder();
        var tripLabel = request.IsFamilyTrip ? "family" : "solo/couple";
        var primaryHotel = hotels.FirstOrDefault();
        var estimatedStayCost = primaryHotel is not null ? primaryHotel.PricePerNight * Math.Max(request.Days - 1, 1) : 0;

        builder.AppendLine($"# {request.Days}-Day {tripLabel} trip to {request.DestinationName}");
        builder.AppendLine($"Party size: {request.PartySize} | Best season: {request.BestTimeToVisit}");
        builder.AppendLine($"Weather: {weather.RecommendationSummary}");
        builder.AppendLine($"Travel advice: {weather.TravelAdvice}");
        builder.AppendLine();

        if (request.MaxBudgetPerNight is not null)
        {
            builder.AppendLine($"Budget target: up to ₹{request.MaxBudgetPerNight:0} per night.");
            if (primaryHotel is not null)
            {
                builder.AppendLine($"Estimated stay cost at {primaryHotel.Name}: ₹{estimatedStayCost:0} for {Math.Max(request.Days - 1, 1)} nights.");
            }

            builder.AppendLine();
        }

        if (hotels.Count > 0)
        {
            builder.AppendLine("## Recommended stays");
            foreach (var hotel in hotels)
            {
                builder.AppendLine($"- **{hotel.Name}** ({hotel.StarRating}★, ₹{hotel.PricePerNight:0}/night)");
                builder.AppendLine($"  {string.Join("; ", hotel.Reasons)}");
            }

            builder.AppendLine();
        }

        if (activities.Count > 0)
        {
            builder.AppendLine("## Recommended experiences");
            foreach (var activity in activities)
            {
                builder.AppendLine($"- **{activity.Name}** — {activity.Category}, {activity.DurationHours}h, {activity.Difficulty}");
                builder.AppendLine($"  {string.Join("; ", activity.Reasons)}");
            }

            builder.AppendLine();
        }

        builder.AppendLine("## Day-by-day plan");
        for (var day = 1; day <= request.Days; day++)
        {
            builder.AppendLine(FormatDayPlan(day, request, weather, primaryHotel, activities, destinationType));
        }

        builder.AppendLine();
        builder.AppendLine("## Travel tips");
        builder.AppendLine(BuildTravelTips(request, weather, destinationType));

        return builder.ToString().TrimEnd();
    }

    private static string FormatDayPlan(
        int day,
        AiItineraryRequest request,
        WeatherForecastResponse weather,
        RecommendedHotel? primaryHotel,
        IReadOnlyList<RecommendedActivity> activities,
        string? destinationType)
    {
        var hotelName = primaryHotel?.Name ?? "your hotel";
        var activity = activities.Count > 0
            ? activities[(day - 1) % activities.Count]
            : null;

        if (day == 1)
        {
            return request.IsFamilyTrip
                ? $"**Day {day}:** Arrive in {request.DestinationName}, check in at {hotelName}, enjoy a relaxed local walk and early dinner suitable for children."
                : $"**Day {day}:** Arrive in {request.DestinationName}, check in at {hotelName}, evening orientation walk and local cuisine.";
        }

        if (day == request.Days)
        {
            return request.IsFamilyTrip
                ? $"**Day {day}:** Easy morning packing, souvenir shopping, casual brunch, and departure."
                : $"**Day {day}:** Final sightseeing or café stop, souvenir shopping, and departure.";
        }

        if (weather.Forecast.Contains("rain", StringComparison.OrdinalIgnoreCase))
        {
            return activity is not null
                ? $"**Day {day}:** Morning indoor/covered activity — {activity.Name}. Afternoon rest at {hotelName} with flexible backup plans due to rain."
                : $"**Day {day}:** Flexible indoor cultural visit and relaxed café time (rain-friendly plan).";
        }

        if (activity is not null)
        {
            var mealSuggestion = destinationType?.ToLowerInvariant() switch
            {
                "beach" => "seafood lunch by the coast",
                "historical" => "heritage-style local thali",
                "city" => "street-food tasting tour",
                "nature" => "organic farm-to-table lunch",
                "adventure" => "hearty local dinner after the activity",
                _ => "regional specialty lunch"
            };

            return request.IsFamilyTrip
                ? $"**Day {day}:** {activity.Name} ({activity.Difficulty}, {activity.DurationHours}h) with breaks as needed, then {mealSuggestion}."
                : $"**Day {day}:** {activity.Name} ({activity.Category}) followed by {mealSuggestion} and free evening.";
        }

        return $"**Day {day}:** Explore highlights of {request.DestinationName} at a comfortable pace with mixed culture and leisure.";
    }

    private static string BuildTravelTips(AiItineraryRequest request, WeatherForecastResponse weather, string? destinationType)
    {
        var tips = new List<string> { weather.TravelAdvice };

        if (request.IsFamilyTrip)
        {
            tips.Add("Plan shorter activity blocks and keep hydration/snacks handy for children.");
        }

        if (request.MaxBudgetPerNight is not null)
        {
            tips.Add($"Book stays early to secure rates near your ₹{request.MaxBudgetPerNight:0}/night target.");
        }

        if (destinationType?.Equals("Adventure", StringComparison.OrdinalIgnoreCase) == true)
        {
            tips.Add("Confirm adventure activity operators a day ahead for weather-dependent slots.");
        }

        if (!weather.IsInBestSeason)
        {
            tips.Add($"For ideal weather, consider traveling during {weather.BestTimeToVisit}.");
        }

        return string.Join("\n", tips.Select(tip => $"- {tip}"));
    }
}
