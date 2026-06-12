using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace TravelRecommendationEngine.Tests;

public sealed class ApiIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public ApiIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Recommendations_ReturnsExplanationsAndScoreBreakdown()
    {
        var response = await _client.PostAsJsonAsync("/api/recommendations", new { userId = 1, maxBudgetPerNight = 180 });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var first = document.RootElement.GetProperty("recommendations")[0];
        Assert.True(first.GetProperty("explanations").GetArrayLength() > 0);
        Assert.True(first.TryGetProperty("scoreBreakdown", out _));
        Assert.True(first.GetProperty("recommendedHotels")[0].TryGetProperty("reasons", out _));
    }

    [Fact]
    public async Task AnalyticsSummary_ReturnsPreferenceStatistics()
    {
        await _client.PostAsJsonAsync("/api/recommendations", new { userId = 1 });
        var response = await _client.GetAsync("/api/analytics/summary");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.True(document.RootElement.GetProperty("totalRecommendationRequests").GetInt32() >= 1);
    }

    [Fact]
    public async Task Weather_ReturnsStructuredRecommendationSummary()
    {
        var response = await _client.GetAsync("/api/weather/Goa%20Beaches");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.False(string.IsNullOrWhiteSpace(document.RootElement.GetProperty("recommendationSummary").GetString()));
    }

    [Fact]
    public async Task Recommendations_CompletesWithinAcceptableTime()
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var response = await _client.PostAsJsonAsync("/api/recommendations", new { userId = 2 });
        stopwatch.Stop();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(stopwatch.ElapsedMilliseconds < 5000);
    }
}
