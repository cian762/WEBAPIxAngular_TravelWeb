namespace TravelWeb_API.Models.Itinerary.DTO
{
    public class ItineraryResponseDto
    {
        public int ItineraryId { get; set; }
        public string MemberId { get; set; }
        public string ItineraryName { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string Introduction { get; set; }


    }
}
