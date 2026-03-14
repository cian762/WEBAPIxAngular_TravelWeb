namespace TravelWeb_API.Models.TripProduct.TripDTO
{
    public class OrderDetailDto
    {
        // 主表資訊
        public int OrderId { get; set; }
        public string ContactName { get; set; } = null!;
        public string ContactPhone { get; set; } = null!;
        public string ContactEmail { get; set; } = null!;
        public string? CustomerNote { get; set; }
        public decimal TotalAmount { get; set; }
        public string OrderStatus { get; set; } = null!;
        public string PaymentStatus { get; set; } = null!;
        public DateTime CreatedAt { get; set; }

        // 商品明細清單
        public List<OrderItemDetailDto> Items { get; set; } = new();
    }
    public class OrderItemDetailDto
    {
        public string ProductCode { get; set; } = null!;
        public string ProductName { get; set; } = null!; // 來自快照
        public string? ProductImage { get; set; }        // 來自 TripProducts 關聯

        // 該商品下的票種明細 (例如：成人票 x2, 學生票 x1)
        public List<OrderTicketDetailDto> Tickets { get; set; } = new();
    }

    public class OrderTicketDetailDto
    {
        public string TicketName { get; set; } = null!;  // 來自快照
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }           // 來自快照
        public decimal SubTotal => Quantity * UnitPrice; // 小計
    }
}
