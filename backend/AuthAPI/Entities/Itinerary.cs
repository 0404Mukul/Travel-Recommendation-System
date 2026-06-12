namespace AuthAPI.Entities
{
    public class Itinerary
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public string Destination { get; set; } = string.Empty;

        public decimal Budget { get; set; }

        public string Interests { get; set; } = string.Empty;

        public string WeatherInfo { get; set; } = string.Empty;

        public string Recommendations { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public User User { get; set; } = null!;

    
        public ICollection<SharedLink> SharedLinks
        {
            get;
            set;
        } = new List<SharedLink>();
    }
}