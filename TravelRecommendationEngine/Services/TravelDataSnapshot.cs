using TravelRecommendationEngine.Models;

namespace TravelRecommendationEngine.Services;

public sealed class TravelDataSnapshot
{
    public TravelData Data { get; init; } = new();
    public IReadOnlyDictionary<int, User> UsersById { get; init; } = new Dictionary<int, User>();
    public IReadOnlyDictionary<int, Destination> BestDestinationById { get; init; } = new Dictionary<int, Destination>();
    public IReadOnlyDictionary<string, Destination> BestDestinationByName { get; init; } = new Dictionary<string, Destination>(StringComparer.OrdinalIgnoreCase);
    public ILookup<int, Hotel> HotelsByDestinationId { get; init; } = Array.Empty<Hotel>().ToLookup(_ => 0);
    public ILookup<int, Activity> ActivitiesByDestinationId { get; init; } = Array.Empty<Activity>().ToLookup(_ => 0);
    public IReadOnlyDictionary<int, ReviewStats> ReviewStatsByDestinationId { get; init; } = new Dictionary<int, ReviewStats>();
    public ILookup<int, int> VisitedDestinationIdsByUserId { get; init; } = Array.Empty<int>().ToLookup(_ => 0);

    public static TravelDataSnapshot Create(TravelData data)
    {
        var usersById = data.Users.ToDictionary(user => user.UserId);
        var bestDestinationById = data.Destinations
            .GroupBy(destination => destination.DestinationId)
            .ToDictionary(group => group.Key, group => group.OrderByDescending(d => d.Popularity).First());

        var bestDestinationByName = data.Destinations
            .GroupBy(destination => destination.Name, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.OrderByDescending(d => d.Popularity).First(), StringComparer.OrdinalIgnoreCase);

        var reviewStats = data.Reviews
            .GroupBy(review => review.DestinationId)
            .ToDictionary(
                group => group.Key,
                group => new ReviewStats(group.Average(review => review.Rating), group.Count()));

        var visitedByUser = data.TravelHistories.ToLookup(history => history.UserId, history => history.DestinationId);

        return new TravelDataSnapshot
        {
            Data = data,
            UsersById = usersById,
            BestDestinationById = bestDestinationById,
            BestDestinationByName = bestDestinationByName,
            HotelsByDestinationId = data.Hotels.ToLookup(hotel => hotel.DestinationId),
            ActivitiesByDestinationId = data.Activities.ToLookup(activity => activity.DestinationId),
            ReviewStatsByDestinationId = reviewStats,
            VisitedDestinationIdsByUserId = visitedByUser
        };
    }
}

public readonly record struct ReviewStats(double AverageRating, int Count);
