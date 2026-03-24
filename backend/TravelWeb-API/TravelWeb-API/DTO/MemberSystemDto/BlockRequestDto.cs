using System.ComponentModel.DataAnnotations;

namespace TravelWeb_API.DTO.MemberSystemDto
{
    public class BlockRequestDto
    {
        [Required(ErrorMessage = "請提供要封鎖的會員 ID")]
        public string BlockedId { get; set; }

        // 封鎖原因 (非必填，可以讓使用者輸入為什麼想封鎖他)
        [MaxLength(100, ErrorMessage = "封鎖原因最多 100 個字")]
        public string? Reason { get; set; }
    }
}