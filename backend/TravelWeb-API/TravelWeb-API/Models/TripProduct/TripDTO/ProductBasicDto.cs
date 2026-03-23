namespace TravelWeb_API.Models.TripProduct.TripDTO
{
    public class ProductBasicDto
    {
        public int TripProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? CoverImage { get; set; }
        public string? RegionName { get; set; }
        public string? Description { get; set; }
    
        public List<string> Tags { get; set; } = new List<string>();
    }
    public class ProductItineraryDto
    {
        public int DayNumber { get; set; } // 第幾天
        public int SortOrder { get; set; } // 排序
        public string? CustomText { get; set; } // 你的「測試啊」、「燈會」等文字
        public List<string> ResourceUrls { get; set; } = new List<string>();//抓多圖
        public string?ResourceName { get; set; } // 如果有連到活動表，可以帶名稱
        public string? DefaultDescription { get; set; }
    }
    public class ProductScheduleDto
    {
        public string ProductCode { get; set; } = string.Empty;
        public DateOnly StartDate { get; set; }
        public decimal Price { get; set; }
        public int AvailableStock { get; set; }
        public string StockStatus => AvailableStock > 0
            ? (AvailableStock < 10 ? $"最後 {AvailableStock} 位" : "充足")
            : "已售罄";
    }
}
