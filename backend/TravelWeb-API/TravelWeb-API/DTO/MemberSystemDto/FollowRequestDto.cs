using System.ComponentModel.DataAnnotations;

namespace TravelWeb_API.DTO.MemberSystemDto
{
    public class FollowRequestDto
    {
        [Required(ErrorMessage = "請提供要追隨的會員 ID")]
        public string FollowedId { get; set; }
    }
}