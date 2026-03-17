namespace TravelWeb_API.Models.Itinerary.DTO
{
    public class ItineraryVersionHistoryDto
    {
        public int VersionId { get; set; }
        public int VersionNumber { get; set; }
        public string? VersionRemark { get; set; }
        public DateTime CreateTime { get; set; }
        public string? Source { get; set; }
        public bool IsCurrent { get; set; } // 是否為目前使用中的版本 (CurrentUsageStatus == "Y")
    }

}


