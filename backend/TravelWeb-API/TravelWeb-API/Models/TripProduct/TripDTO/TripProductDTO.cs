namespace TravelWeb_API.Models.TripProduct.TripDTO
{
    public class TripProductDTO
    {
        //這是行程商品表的顯示DTO
        public int TripProductId { get; set; }
        public string? ProductName { get; set; }
        public string ?CoverImage { get; set; }
        public decimal? DisplayPrice { get; set; }
        public string ?RegionName { get; set; } // 從 Region 表 Join 過來
        public string ?CategoryTag { get; set; } // 例如 "冬季賞雪"
        public int? DurationDays { get; set; }


    }
    //搜尋條件
    public class ProductQueryDTO
    {
        public List<int>?TagIds { get; set; } // 存放勾選的標籤 ID
        public int? RegionId { get; set; }    // 對應你圖中的 RegionID
        public string ?Keyword { get; set; }   // 關鍵字搜尋
        public decimal? MinPrice { get; set; } // 預算下限
        public decimal? MaxPrice { get; set; } // 預算上限

        // 分頁必備
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 12;

        // 排序方式 (例如: 1 = 價格由低到高, 2 = 點擊次數由高到低)
        public int? SortType { get; set; }
        // 地區通常是單選（如：日本）
    }
}
