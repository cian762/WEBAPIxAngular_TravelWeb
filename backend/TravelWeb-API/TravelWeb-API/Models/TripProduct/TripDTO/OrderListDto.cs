namespace TravelWeb_API.Models.TripProduct.TripDTO
{
    public class OrderListDto
    {
        public int OrderId { get; set; }
        public DateTime CreatedAt { get; set; }
        public decimal TotalAmount { get; set; }
        public string OrderStatus { get; set; } = null!;   // Pending, Completed, Cancelled
        public string PaymentStatus { get; set; } = null!; // Unpaid, Paid

        // 列表顯示用：第一件商品的名稱與圖片
        public string FirstItemName { get; set; } = null!;
        public string? FirstItemImage { get; set; }
        public int TotalItemCount { get; set; } // 總共買了幾項商品
       
    }
}
