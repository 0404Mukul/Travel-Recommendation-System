using TravelRecommendationEngine.Services;

namespace TravelRecommendationEngine.Tests;

public sealed class PromptBuilderServiceTests
{
    [Fact]
    public void BuildItineraryPrompt_ReturnsExpectedPrompt()
    {
        var service = new PromptBuilderService();

        var prompt = service.BuildItineraryPrompt("Goa Beaches", "Nov-Mar", 5, true);

        Assert.Equal("Create a 5-day itinerary for Goa Beaches during Nov-Mar for a family trip.", prompt);
    }
}
