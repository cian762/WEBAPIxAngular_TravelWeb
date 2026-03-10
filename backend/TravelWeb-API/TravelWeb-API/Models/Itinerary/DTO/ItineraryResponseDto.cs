namespace TravelWeb_API.Models.Itinerary.DTO
{
    public class ItineraryResponseDto
    {
        public int ItineraryId { get; set; }
        public string ItineraryName { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public int TotalDays { get; set; } // 這是計算出來的，DB 可能沒這欄位
        public List<ItineraryItemDto> Items { get; set; } // 關聯的景點簡略資訊
    }
}
