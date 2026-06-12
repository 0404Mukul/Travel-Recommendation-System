namespace TravelRecommendationEngine.Services;

public interface IUserPreferenceService
{
    IReadOnlyList<string> GetPreferencesForUser(int userId);
}
