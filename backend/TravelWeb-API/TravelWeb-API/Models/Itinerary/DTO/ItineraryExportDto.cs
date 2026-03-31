namespace TravelWeb_API.Models.Itinerary.DTO
{
    public class ItineraryExportDto
    {
        public string ItineraryName { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string Introduction { get; set; }
        // 包含每日行程清單
        public List<ExportDayDetailDto> Days { get; set; }
    }
    public class ExportDayDetailDto
    {
        public int DayNumber { get; set; }
        public List<ExportItemDto> Items { get; set; }
    }
    public class ExportItemDto
    {
        public string AttractionName { get; set; } // 來自 ItineraryItems 或 Attractions
        public string StartTime { get; set; }
        public string ContentDescription { get; set; }
        public string Activity { get; set; }
    }
}
