namespace TravelRecommendationEngine.Services;

public sealed class UserPreferenceService : IUserPreferenceService
{
    private readonly ICsvLoaderService _csvLoaderService;

    public UserPreferenceService(ICsvLoaderService csvLoaderService)
    {
        _csvLoaderService = csvLoaderService;
    }

    public IReadOnlyList<string> GetPreferencesForUser(int userId)
    {
        var user = _csvLoaderService.LoadTravelData().Users.FirstOrDefault(candidate => candidate.UserId == userId);
        if (user is null)
        {
            throw new KeyNotFoundException($"User with ID {userId} was not found.");
        }

        return user.Preferences
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToList();
    }
}
