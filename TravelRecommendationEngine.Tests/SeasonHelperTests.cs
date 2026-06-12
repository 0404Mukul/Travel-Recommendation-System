using TravelRecommendationEngine.Services;

namespace TravelRecommendationEngine.Tests;

public sealed class SeasonHelperTests
{
    [Theory]
    [InlineData("Nov-Mar", "2025-12-01", true)]
    [InlineData("Nov-Mar", "2025-07-01", false)]
    [InlineData("Apr-Jun,Sep-Nov", "2025-05-15", true)]
    public void IsInBestSeason_ReturnsExpectedResult(string bestTimeToVisit, string referenceDate, bool expected)
    {
        var date = DateOnly.Parse(referenceDate);

        var result = SeasonHelper.IsInBestSeason(bestTimeToVisit, date);

        Assert.Equal(expected, result);
    }
}
