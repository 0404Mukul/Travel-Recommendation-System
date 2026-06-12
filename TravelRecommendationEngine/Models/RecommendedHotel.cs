namespace TravelRecommendationEngine.Models;

public sealed class RecommendedHotel
{
    public int HotelId { get; init; }
    public int DestinationId { get; init; }
    public string Name { get; init; } = string.Empty;
    public int StarRating { get; init; }
    public double PricePerNight { get; init; }
    public string Amenities { get; init; } = string.Empty;
    public double RecommendationScore { get; init; }
    public IReadOnlyList<string> Reasons { get; init; } = [];

    public static RecommendedHotel FromHotel(Hotel hotel, double score, IReadOnlyList<string> reasons) =>
        new()
        {
            HotelId = hotel.HotelId,
            DestinationId = hotel.DestinationId,
            Name = hotel.Name,
            StarRating = hotel.StarRating,
            PricePerNight = hotel.PricePerNight,
            Amenities = hotel.Amenities,
            RecommendationScore = Math.Round(score, 2),
            Reasons = reasons
        };
}
