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
}
