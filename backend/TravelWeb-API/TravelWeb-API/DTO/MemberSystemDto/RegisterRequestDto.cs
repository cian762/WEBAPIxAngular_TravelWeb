using Microsoft.AspNetCore.Http; // 為了使用 IFormFile
using System.ComponentModel.DataAnnotations;

namespace TravelWeb_API.DTO.MemberSystemDto
{
    public class RegisterRequestDto
    {
        // --- Member_List 需求欄位 ---
        [Required(ErrorMessage = "Email 為必填欄位")]
        [EmailAddress(ErrorMessage = "Email 格式不正確")]
        public string Email { get; set; }

        [Required(ErrorMessage = "密碼為必填欄位")]
        [MinLength(8, ErrorMessage = "密碼長度必須至少 8 個字元")]
        // (?=.*[A-Za-z])：至少包含一個英文字母
        // (?=.*\d)：至少包含一個數字
        // .{8,}：長度至少 8，允許包含特殊符號 (比只能英數組合更安全)
        [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d).{8,}$",
            ErrorMessage = "密碼必須為英數混合 (至少包含一個英文字母與一個數字)")]
        public string Password { get; set; }

        [Required(ErrorMessage = "電話為必填欄位")]
        public string Phone { get; set; }

        // --- Member_Information 需求欄位 ---
        [Required(ErrorMessage = "姓名為必填欄位")]
        public string Name { get; set; }

        // 性別 (1=男, 2=女)
        public byte? Gender { get; set; }

        // 出生年月日
        public DateOnly? BirthDate { get; set; }

        // 大頭貼 (唯一非必填)
        public IFormFile? AvatarFile { get; set; }
    }
}