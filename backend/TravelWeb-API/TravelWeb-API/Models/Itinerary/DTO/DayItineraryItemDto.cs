namespace TravelWeb_API.Models.Itinerary.DTO
{
    public class DayItineraryItemDto
    {
        public int Order { get; set; }
        public string? PlaceId { get; set; }
        public string? PlaceName { get; set; }
        public string? Address { get; set; }
        public TimeSpan? ArrivalTime { get; set; }
        public TimeSpan? DepartureTime { get; set; }
    }
    public class DayItineraryDto
    {
        public int DayNumber { get; set; }
        public List<DayItineraryItemDto> Items { get; set; } = new();
    }
}
