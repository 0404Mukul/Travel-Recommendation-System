using Microsoft.Extensions.Options;
using TravelRecommendationEngine.Configuration;
using TravelRecommendationEngine.Models;

namespace TravelRecommendationEngine.Services;

public sealed class RecommendationService : IRecommendationService
{
    private readonly ICsvLoaderService _csvLoaderService;
    private readonly IDestinationService _destinationService;
    private readonly IUserPreferenceService _userPreferenceService;
    private readonly IWeatherService _weatherService;
    private readonly IHotelService _hotelService;
    private readonly IActivityService _activityService;
    private readonly IAnalyticsService _analyticsService;
    private readonly RecommendationSettings _settings;

    public RecommendationService(
        ICsvLoaderService csvLoaderService,
        IDestinationService destinationService,
        IUserPreferenceService userPreferenceService,
        IWeatherService weatherService,
        IHotelService hotelService,
        IActivityService activityService,
        IAnalyticsService analyticsService,
        IOptions<RecommendationSettings> settings)
    {
        _csvLoaderService = csvLoaderService;
        _destinationService = destinationService;
        _userPreferenceService = userPreferenceService;
        _weatherService = weatherService;
        _hotelService = hotelService;
        _activityService = activityService;
        _analyticsService = analyticsService;
        _settings = settings.Value;
    }

    public RecommendationResponse GetRecommendations(RecommendationRequest request)
    {
        var snapshot = _csvLoaderService.GetSnapshot();
        var user = snapshot.UsersById.GetValueOrDefault(request.UserId)
            ?? throw new KeyNotFoundException($"User with ID {request.UserId} was not found.");

        var preferences = _userPreferenceService.GetPreferencesForUser(request.UserId);
        var context = new RecommendationContext
        {
            User = user,
            Preferences = preferences,
            MaxBudgetPerNight = request.MaxBudgetPerNight ?? _settings.DefaultMaxBudgetPerNight
        };

        var normalizedPreferences = PreferenceNormalizer.ToSet(preferences);
        var visitedDestinations = snapshot.VisitedDestinationIdsByUserId[request.UserId].ToHashSet();

        var matchingDestinations = _destinationService.GetDestinationsMatchingPreferences(preferences)
            .GroupBy(destination => destination.Name, StringComparer.OrdinalIgnoreCase)
            .Select(group => group.OrderByDescending(destination => destination.Popularity).First())
            .ToList();

        var recommendations = matchingDestinations
            .Select(destination =>
            {
                ReviewStats? reviewStats = snapshot.ReviewStatsByDestinationId.TryGetValue(destination.DestinationId, out var stats)
                    ? stats
                    : null;
                var weather = _weatherService.GetWeather(destination.Name);
                var hotels = _hotelService.GetRecommendedHotels(destination.DestinationId, context);
                var activities = _activityService.GetRecommendedActivities(destination.DestinationId, context);
                var scoreResult = RecommendationScoringEngine.ScoreDestination(
                    destination,
                    reviewStats,
                    weather,
                    hotels,
                    activities,
                    normalizedPreferences,
                    visitedDestinations.Contains(destination.DestinationId),
                    context,
                    _settings);

                return new RecommendationItem
                {
                    DestinationId = destination.DestinationId,
                    DestinationName = destination.Name,
                    Type = destination.Type,
                    Popularity = Math.Round(destination.Popularity, 2),
                    AverageRating = Math.Round(reviewStats?.AverageRating ?? 0.0, 2),
                    RecommendationScore = Math.Round(scoreResult.RawScore * 10, 2),
                    BestTimeToVisit = destination.BestTimeToVisit,
                    Weather = weather,
                    RecommendedHotels = hotels,
                    RecommendedActivities = activities,
                    Explanations = scoreResult.Explanations,
                    ScoreBreakdown = scoreResult.Breakdown
                };
            })
            .OrderByDescending(recommendation => recommendation.RecommendationScore)
            .ThenByDescending(recommendation => recommendation.AverageRating)
            .ThenBy(recommendation => recommendation.DestinationName)
            .Take(_settings.DefaultRecommendationCount)
            .ToList();

        _analyticsService.TrackRecommendationRequest(request.UserId, recommendations, context.MaxBudgetPerNight);

        return new RecommendationResponse { Recommendations = recommendations };
    }
}
