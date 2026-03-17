namespace TravelWebApi.DTOs
{
    public class MemberProfileResponseDto
    {
        public string MemberId { get; set; } = null!;
        public string? Name { get; set; }
        public string? AvatarUrl { get; set; }
        public string? Gender { get; set; }
        public DateOnly? BirthDate { get; set; } 
        public string Email { get; set; } = null!;
        public string? Phone { get; set; }
    }
}
