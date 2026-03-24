using Microsoft.AspNetCore.Http;

namespace TravelWebApi.DTOs
{
    public class MemberProfileUpdateDto
    {
        public string? Name { get; set; }
        // 🔥 修正：接收實體檔案請命名為 AvatarFile，不要加上 Url
        public IFormFile? AvatarFile { get; set; }
    }
}