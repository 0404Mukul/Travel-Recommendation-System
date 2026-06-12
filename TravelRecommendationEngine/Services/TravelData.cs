using TravelRecommendationEngine.Models;

namespace TravelRecommendationEngine.Services;

public sealed class TravelData
{
    public IReadOnlyList<Destination> Destinations { get; init; } = [];
    public IReadOnlyList<User> Users { get; init; } = [];
    public IReadOnlyList<Review> Reviews { get; init; } = [];
    public IReadOnlyList<TravelHistory> TravelHistories { get; init; } = [];
    public IReadOnlyList<Hotel> Hotels { get; init; } = [];
    public IReadOnlyList<Activity> Activities { get; init; } = [];
}
