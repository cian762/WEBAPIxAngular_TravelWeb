using System.ComponentModel.DataAnnotations;

namespace TravelWeb_API.Models.TripProduct.TripDTO
{
    public  class CreateOrderDto
    {
       
        public string ?MemberId { get; set; } = null!;

        public string ?ContactName { get; set; } = null!;

        public string ?ContactPhone { get; set; } = null!;

   
        public string ?ContactEmail { get; set; } = null!;

        // 備註是選填，所以不用加 Required，也不用 null!
        public string? CustomerNote { get; set; }
        // 關鍵：如果是直接購買，前端要把商品資訊傳過來
        // 如果是購物車結帳，這個 List 就是空的或 null
        public List<AddToCartDTO>? DirectBuyItems { get; set; }
    }
}
