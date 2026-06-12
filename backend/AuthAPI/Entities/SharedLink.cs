namespace AuthAPI.Entities
{
    public class SharedLink
    {
        public int Id {get; set;}

        public int ItineraryId {get;set;}

        public string PublicToken {get;set;} = string.Empty;

        public DateTime CreatedAt {get;set;} = DateTime.UtcNow;

        public Itinerary Itinerary {get;set;} = null!;

    }
}