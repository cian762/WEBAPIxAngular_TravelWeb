using System.ComponentModel.DataAnnotations;

namespace TravelWebApi.DTOs // 替換成你的專案命名空間
{
    public class ComplaintCreateDto
    {
        [Required(ErrorMessage = "請填寫投訴內容")]
        public string Description { get; set; } = null!;

        [Required(ErrorMessage = "請填寫希望回覆的信箱")]
        [EmailAddress(ErrorMessage = "信箱格式錯誤")]
        public string ReplyEmail { get; set; } = null!;
    }
}