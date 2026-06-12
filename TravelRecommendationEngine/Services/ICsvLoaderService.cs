namespace TravelRecommendationEngine.Services;

public interface ICsvLoaderService
{
    TravelData LoadTravelData();
    TravelDataSnapshot GetSnapshot();
}
