namespace TravelRecommendationEngine.Models;

public sealed class Hotel
{
    public int HotelId { get; init; }
    public int DestinationId { get; init; }
    public string Name { get; init; } = string.Empty;
    public int StarRating { get; init; }
    public double PricePerNight { get; init; }
    public string Amenities { get; init; } = string.Empty;
}
