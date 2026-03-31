namespace TravelWebApi.DTOs
{
    public class MemberProfileResponseDto
    {
        public string MemberCode { get; set; } = null!; 
        public string MemberId { get; set; } = null!;
        public string? Name { get; set; }
        public string? AvatarUrl { get; set; }
        public byte? Gender { get; set; } 
        public string? BackgroundUrl { get; set; }
        public DateOnly? BirthDate { get; set; }
        public string Email { get; set; } = null!;
        public string? Phone { get; set; }
        public string? Status { get; set; } 
    }
}