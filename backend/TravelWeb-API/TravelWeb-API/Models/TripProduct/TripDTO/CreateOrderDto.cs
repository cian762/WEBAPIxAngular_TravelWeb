using System.ComponentModel.DataAnnotations;

namespace TravelWeb_API.Models.TripProduct.TripDTO
{
    public  class CreateOrderDto
    {
        [Required(ErrorMessage = "會員ID不可為空")]
        public string MemberId { get; set; } = null!;

        [Required(ErrorMessage = "請填寫聯絡人姓名")]
        public string ContactName { get; set; } = null!;

        [Required(ErrorMessage = "請填寫聯絡電話")]
        [Phone(ErrorMessage = "電話格式錯誤")]
        public string ContactPhone { get; set; } = null!;

        [Required(ErrorMessage = "請填寫 Email")]
        [EmailAddress(ErrorMessage = "Email 格式錯誤")]
        public string ContactEmail { get; set; } = null!;

        // 備註是選填，所以不用加 Required，也不用 null!
        public string? CustomerNote { get; set; }
        // 關鍵：如果是直接購買，前端要把商品資訊傳過來
        // 如果是購物車結帳，這個 List 就是空的或 null
        public List<AddToCartDTO>? DirectBuyItems { get; set; }
    }
}
