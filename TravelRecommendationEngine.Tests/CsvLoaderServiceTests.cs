using TravelRecommendationEngine.Services;

namespace TravelRecommendationEngine.Tests;

public sealed class CsvLoaderServiceTests
{
    [Fact]
    public void LoadTravelData_MapsAllDatasetFiles()
    {
        var dataPath = Path.Combine(AppContext.BaseDirectory, "Data");
        var service = new CsvLoaderService(dataPath);

        var data = service.LoadTravelData();

        Assert.NotEmpty(data.Destinations);
        Assert.NotEmpty(data.Users);
        Assert.NotEmpty(data.Reviews);
        Assert.NotEmpty(data.TravelHistories);
        Assert.All(data.Users.Take(5), user => Assert.True(user.UserId > 0));
        Assert.All(data.Destinations.Take(5), destination => Assert.False(string.IsNullOrWhiteSpace(destination.Type)));
    }
}
