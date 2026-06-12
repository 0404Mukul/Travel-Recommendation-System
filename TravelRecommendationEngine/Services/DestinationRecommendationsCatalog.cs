using TravelRecommendationEngine.Models;

namespace TravelRecommendationEngine.Services;

internal static class DestinationRecommendationsCatalog
{
    public static IReadOnlyList<Hotel> CreateHotels(Destination destination, int maxRecommendations)
    {
        var templates = GetHotelTemplates(destination.Type);
        return templates
            .Select((template, index) => new Hotel
            {
                HotelId = destination.DestinationId * 10 + index + 1,
                DestinationId = destination.DestinationId,
                Name = $"{template.NamePrefix} {destination.State}",
                StarRating = template.StarRating,
                PricePerNight = template.BasePrice + index * 15,
                Amenities = template.Amenities
            })
            .Take(maxRecommendations)
            .ToList();
    }

    public static IReadOnlyList<Activity> CreateActivities(Destination destination, int maxRecommendations, bool preferFamilyFriendly)
    {
        var templates = GetActivityTemplates(destination.Type);
        var ordered = preferFamilyFriendly
            ? templates.OrderByDescending(template => template.Difficulty.Equals("Easy", StringComparison.OrdinalIgnoreCase))
            : templates.AsEnumerable();

        return ordered
            .Select((template, index) => new Activity
            {
                ActivityId = destination.DestinationId * 10 + index + 1,
                DestinationId = destination.DestinationId,
                Name = template.Name,
                Category = template.Category,
                DurationHours = template.DurationHours,
                Difficulty = template.Difficulty,
                Popularity = template.Popularity
            })
            .Take(maxRecommendations)
            .ToList();
    }

    public static WeatherForecastResponse CreateWeatherFallback(Destination destination)
    {
        var (forecast, temperature) = destination.Type.ToLowerInvariant() switch
        {
            "beach" => ("Sunny with pleasant coastal breeze", 29.0),
            "historical" => ("Dry and comfortable for sightseeing", 26.0),
            "city" => ("Partly cloudy with mild urban temperatures", 25.0),
            "nature" => ("Cool with refreshing greenery", 22.0),
            "adventure" => ("Crisp mountain air with clear visibility", 18.0),
            _ => ("Mild and suitable for travel", 24.0)
        };

        return new WeatherForecastResponse
        {
            DestinationName = destination.Name,
            BestTimeToVisit = destination.BestTimeToVisit,
            Forecast = forecast,
            TemperatureCelsius = temperature
        };
    }

    private static IReadOnlyList<HotelTemplate> GetHotelTemplates(string destinationType) =>
        destinationType.ToLowerInvariant() switch
        {
            "beach" =>
            [
                new HotelTemplate("Seaview Resort", 5, 130, "Pool;Spa;Beach Access"),
                new HotelTemplate("Beachside Inn", 4, 95, "Breakfast;Sea View"),
                new HotelTemplate("Coastal Retreat", 3, 70, "Garden;Wi-Fi")
            ],
            "historical" =>
            [
                new HotelTemplate("Heritage Palace", 5, 155, "Pool;Guided Tours;Historic Architecture"),
                new HotelTemplate("Royal Court Hotel", 4, 115, "Cultural Dining;City View"),
                new HotelTemplate("Monument Stay", 3, 85, "Local Cuisine;Museum Shuttle")
            ],
            "city" =>
            [
                new HotelTemplate("Urban Grand", 5, 145, "Rooftop Bar;Concierge;City View"),
                new HotelTemplate("City Central", 4, 105, "Metro Access;Business Center"),
                new HotelTemplate("Boutique Square", 3, 80, "Cafe;Shopping District")
            ],
            "nature" =>
            [
                new HotelTemplate("Eco Lodge", 4, 120, "Garden;Nature Trails;Organic Dining"),
                new HotelTemplate("Riverside Retreat", 4, 110, "Boat Dock;Bird Watching"),
                new HotelTemplate("Hilltop Haven", 3, 90, "Scenic View;Hiking Access")
            ],
            "adventure" =>
            [
                new HotelTemplate("Mountain Base Camp", 4, 100, "Gear Rental;Trail Maps"),
                new HotelTemplate("Adventure Lodge", 4, 95, "Bonfire;Guided Expeditions"),
                new HotelTemplate("Trail Hostel", 3, 60, "Shared Lounge;Equipment Storage")
            ],
            _ =>
            [
                new HotelTemplate("Traveler's Rest", 4, 100, "Wi-Fi;Restaurant"),
                new HotelTemplate("Comfort Inn", 3, 75, "Parking;Breakfast")
            ]
        };

    private static IReadOnlyList<ActivityTemplate> GetActivityTemplates(string destinationType) =>
        destinationType.ToLowerInvariant() switch
        {
            "beach" =>
            [
                new ActivityTemplate("Sunset Cruise", "Relaxation", 3, "Easy", 9.0),
                new ActivityTemplate("Beach Yoga", "Wellness", 1.5, "Easy", 8.4),
                new ActivityTemplate("Water Sports", "Adventure", 2, "Moderate", 8.8)
            ],
            "historical" =>
            [
                new ActivityTemplate("Heritage Walk", "Cultural", 3, "Easy", 8.9),
                new ActivityTemplate("Fort Tour", "Cultural", 4, "Moderate", 8.7),
                new ActivityTemplate("Museum Visit", "Cultural", 2, "Easy", 7.8)
            ],
            "city" =>
            [
                new ActivityTemplate("Food Tour", "Culinary", 3, "Easy", 8.6),
                new ActivityTemplate("Shopping District Walk", "Leisure", 2, "Easy", 8.0),
                new ActivityTemplate("Night Market Visit", "Cultural", 2.5, "Easy", 8.3)
            ],
            "nature" =>
            [
                new ActivityTemplate("Nature Trail", "Nature", 3, "Easy", 8.5),
                new ActivityTemplate("Wildlife Safari", "Nature", 4, "Moderate", 8.9),
                new ActivityTemplate("Boat Ride", "Relaxation", 2, "Easy", 8.2)
            ],
            "adventure" =>
            [
                new ActivityTemplate("Mountain Trek", "Adventure", 5, "Hard", 9.1),
                new ActivityTemplate("River Rafting", "Adventure", 3, "Moderate", 8.8),
                new ActivityTemplate("Paragliding", "Adventure", 2, "Hard", 9.0)
            ],
            _ =>
            [
                new ActivityTemplate("Local Sightseeing", "Cultural", 3, "Easy", 8.0),
                new ActivityTemplate("Market Exploration", "Leisure", 2, "Easy", 7.5)
            ]
        };

    private sealed record HotelTemplate(string NamePrefix, int StarRating, double BasePrice, string Amenities);
    private sealed record ActivityTemplate(string Name, string Category, double DurationHours, string Difficulty, double Popularity);
}
