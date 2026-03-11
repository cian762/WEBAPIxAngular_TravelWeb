namespace TravelWeb_API.Models.Itinerary.DTO
{
    public class ItinerarySnapshotDto
    {
        public int ItineraryId { get; set; }
        public string? VersionNote { get; set; } // 這次儲存的備註，例如「修正第二天行程」

        // 目前畫面上所有的景點清單 (已含 DayNumber 與前端排序順序)
        public List<SnapshotItemDto> Items { get; set; } = new List<SnapshotItemDto>();
    }
    public class SnapshotItemDto
    {
        public int AttractionId { get; set; }
        public int DayNumber { get; set; } // 使用者當前分配的天數
        public string? ContentDescription { get; set; }
        public DateTime? StartTime { get; set; }

        public DateTime? EndTime { get; set; }
    }
}
