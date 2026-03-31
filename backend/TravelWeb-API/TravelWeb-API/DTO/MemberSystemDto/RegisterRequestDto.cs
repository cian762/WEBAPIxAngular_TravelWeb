using Microsoft.AspNetCore.Http; 
using System.ComponentModel.DataAnnotations;

namespace TravelWeb_API.DTO.MemberSystemDto
{
    public class RegisterRequestDto
    {
        [Required(ErrorMessage = "Email 為必填欄位")]
        [EmailAddress(ErrorMessage = "Email 格式不正確")]
        public string Email { get; set; }

        [Required(ErrorMessage = "密碼為必填欄位")]
        [MinLength(8, ErrorMessage = "密碼長度必須至少 8 個字元")]
        [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d).{8,}$",
            ErrorMessage = "密碼必須為英數混合 (至少包含一個英文字母與一個數字)")]
        public string Password { get; set; }

        [Required(ErrorMessage = "電話為必填欄位")]
        public string Phone { get; set; }

        [Required(ErrorMessage = "姓名為必填欄位")]
        public string Name { get; set; }

        public byte? Gender { get; set; }

        public DateOnly? BirthDate { get; set; }

        public IFormFile? AvatarFile { get; set; }
    }
}