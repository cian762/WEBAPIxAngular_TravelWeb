namespace TravelWeb_API.Models.Itinerary.DTO
{
    public class ItineraryDetailDto
    {
        public int ItineraryId { get; set; }
        public string ItineraryName { get; set; }
        public string? ItineraryImage { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string? Introduction { get; set; }
        public string MemberId { get; set; }
        public int VersionId { get; set; }

        // 核心：包含當前使用的版本資訊
        public VersionDto? CurrentVersion { get; set; }
    }
    public class VersionDto
    {
        public int VersionId { get; set; }
        public int VersionNumber { get; set; }
        public string? VersionName { get; set; } // 如果你有命名的話
        public string CurrentUsageStatus { get; set; } // "Y" or "N"

        // 核心：該版本下的所有景點項目清單
        public List<ItemDetailDto> Items { get; set; } = new List<ItemDetailDto>();
    }
    public class ItemDetailDto
    {
        public int ItemId { get; set; }

        public int SortOrder { get; set; }
        public int DayNumber { get; set; }
        public string? ContentDescription { get; set; }

        // 來自 Attractions 表的欄位 (由 LINQ Join 取得)
        public int? AttractionId { get; set; }
        public string AttractionName { get; set; }
        public string? Address { get; set; }
        public string? GooglePlaceId { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string? PlaceId { get; set; }
        public string? PlaceName
        {
            get; set;
        }
    }
}
