namespace TravelRecommendationEngine.Models;

public sealed class DemoScenario
{
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Endpoint { get; init; } = string.Empty;
    public string Method { get; init; } = string.Empty;
    public object SampleRequest { get; init; } = new();
}
