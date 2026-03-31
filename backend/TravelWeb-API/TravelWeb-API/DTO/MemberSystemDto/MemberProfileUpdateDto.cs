using Microsoft.AspNetCore.Http;

namespace TravelWebApi.DTOs
{
    public class MemberProfileUpdateDto
    {
        public string? Name { get; set; }
        public IFormFile? AvatarFile { get; set; }
        public IFormFile? BackgroundFile { get; set; }
    }
}