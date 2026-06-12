using TravelRecommendationEngine.Configuration;
using TravelRecommendationEngine.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<RecommendationSettings>(
    builder.Configuration.GetSection(RecommendationSettings.SectionName));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient<IWeatherService, WeatherService>();

builder.Services.AddSingleton<ICsvLoaderService, CsvLoaderService>();
builder.Services.AddSingleton<IDestinationService, DestinationService>();
builder.Services.AddSingleton<IUserPreferenceService, UserPreferenceService>();
builder.Services.AddSingleton<IHotelService, HotelService>();
builder.Services.AddSingleton<IActivityService, ActivityService>();
builder.Services.AddSingleton<IRecommendationService, RecommendationService>();
builder.Services.AddSingleton<IPromptBuilderService, PromptBuilderService>();
builder.Services.AddSingleton<IItineraryBuilderService, ItineraryBuilderService>();
builder.Services.AddSingleton<IAnalyticsService, AnalyticsService>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Travel Recommendation API V1");
    c.RoutePrefix = "swagger";
});

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapGet("/", () => Results.Ok(new
{
    application = "Travel Recommendation Engine",
    status = "Running",
    endpoints = new[]
    {
        "POST /api/recommendations",
        "GET /api/weather/{destinationName}",
        "GET /api/hotels/{destinationId}",
        "GET /api/activities/{destinationId}",
        "POST /api/recommendations/itinerary-prompt",
        "POST /api/ai/itinerary",
        "GET /api/ai/demo-scenarios",
        "GET /api/analytics/summary",
        "POST /api/analytics/selection"
    },
    sampleRecommendationRequest = new
    {
        url = "/api/recommendations",
        method = "POST",
        body = new { userId = 1, maxBudgetPerNight = 150 }
    },
    sampleAiRequest = new
    {
        url = "/api/ai/itinerary",
        method = "POST",
        body = new
        {
            destinationName = "Goa Beaches",
            bestTimeToVisit = "Nov-Mar",
            days = 5,
            isFamilyTrip = true,
            partySize = 4,
            maxBudgetPerNight = 150
        }
    }
}));

app.MapControllers();

app.Run();

public partial class Program { }
