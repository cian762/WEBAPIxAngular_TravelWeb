using System.ComponentModel.DataAnnotations;

namespace TravelWeb_API.DTO.MemberSystemDto
{
    // ==========================================
    // 📦 用來接收前端「重設密碼」表單的資料格式
    // ==========================================
    public class ResetPasswordRequestDto
    {
        [Required(ErrorMessage = "信箱不可為空")]
        [EmailAddress(ErrorMessage = "信箱格式錯誤")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "請輸入 4 位數驗證碼")]
        [StringLength(4, MinimumLength = 4, ErrorMessage = "驗證碼必須剛好是 4 位數")]
        public string Code { get; set; } = null!;

        [Required(ErrorMessage = "新密碼不可為空")]
        [MinLength(8, ErrorMessage = "密碼長度必須至少 8 個字元")]
        [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d).{8,}$", ErrorMessage = "密碼必須為英數混合")]
        public string NewPassword { get; set; } = null!;
    }
}