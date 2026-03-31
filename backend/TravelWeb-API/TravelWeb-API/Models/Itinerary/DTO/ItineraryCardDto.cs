namespace TravelWeb_API.Models.Itinerary.DTO
{
    public class ItineraryCardDto
    {
        public int ItineraryId { get; set; }
        public string ItineraryName { get; set; } = string.Empty;
        public string? ItineraryImage { get; set; }   // Cloudinary URL
        public string? Introduction { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string? CurrentStatus { get; set; }
    }
}
