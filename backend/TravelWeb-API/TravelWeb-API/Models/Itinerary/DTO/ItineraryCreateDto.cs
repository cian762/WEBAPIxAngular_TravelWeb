namespace TravelWeb_API.Models.Itinerary.DTO
{
    public class ItineraryCreateDto
    {
        public string MemberId { get; set; }
        public string ItineraryName { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string Introduction { get; set; }
        public List<ItineraryItemInputDto> ItemsToPush { get; set; }
    }
    // 1. 外部地點資訊 (來自 Google)
    public class ExternalLocationDto
    {
        public string GooglePlaceId { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
    }

    public class ItineraryItemInputDto
    {
        public int? AttractionId { get; set; } // 若有值，代表是 DB 既有景點
        public ExternalLocationDto ExternalLocation { get; set; } // 若有值，代表是 Google 新景點
        public string UserNote { get; set; } // 使用者自訂備註
    }
}
