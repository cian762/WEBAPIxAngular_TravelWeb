using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace TravelWebApi.DTOs
{
    public class ComplaintCreateDto
    {
        [Required(ErrorMessage = "請選擇投訴主旨")]
        public string Subject { get; set; } = null!;

        [Required(ErrorMessage = "請填寫投訴內容")]
        public string Description { get; set; } = null!;

        [Required(ErrorMessage = "請填寫希望回覆的信箱")]
        [EmailAddress(ErrorMessage = "信箱格式錯誤")]
        public string ReplyEmail { get; set; } = null!;

        public IFormFile? ImageFile { get; set; }
    }
}