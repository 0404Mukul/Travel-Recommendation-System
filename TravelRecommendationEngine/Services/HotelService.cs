using TravelRecommendationEngine.Models;

namespace TravelRecommendationEngine.Services;

public sealed class HotelService : IHotelService
{
    private readonly ICsvLoaderService _csvLoaderService;
    private readonly IDestinationService _destinationService;

    public HotelService(ICsvLoaderService csvLoaderService, IDestinationService destinationService)
    {
        _csvLoaderService = csvLoaderService;
        _destinationService = destinationService;
    }

    public IReadOnlyList<RecommendedHotel> GetRecommendedHotels(
        int destinationId,
        RecommendationContext context,
        int maxRecommendations = 3)
    {
        var snapshot = _csvLoaderService.GetSnapshot();
        var hotels = snapshot.HotelsByDestinationId[destinationId].ToList();

        if (hotels.Count == 0)
        {
            var destination = _destinationService.GetDestinationById(destinationId);
            if (destination is null)
            {
                return [];
            }

            hotels = DestinationRecommendationsCatalog.CreateHotels(destination, maxRecommendations + 2).ToList();
        }

        return HotelRankingEngine.RankHotels(hotels, context, maxRecommendations);
    }
}
