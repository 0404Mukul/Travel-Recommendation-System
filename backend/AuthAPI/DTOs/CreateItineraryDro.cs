namespace AuthAPI.DTOs
{
    public class CreateItineraryDto
    {
        public string Destination { get; set; }
            = string.Empty;

        public decimal Budget { get; set; }

        public string Interests { get; set; }
            = string.Empty;
    }
}