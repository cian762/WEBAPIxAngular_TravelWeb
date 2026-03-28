namespace TravelWebApi.DTOs
{
    public class MemberProfileResponseDto
    {
        public string MemberCode { get; set; } = null!; // 🔥 新增：為了完整回傳
        public string MemberId { get; set; } = null!;
        public string? Name { get; set; }
        public string? AvatarUrl { get; set; }
        public byte? Gender { get; set; } // 🔥 修正：配合資料庫型別，改為 byte? (1 或 2)
        public string? BackgroundUrl { get; set; }
        public DateOnly? BirthDate { get; set; }
        public string Email { get; set; } = null!;
        public string? Phone { get; set; }
        public string? Status { get; set; } // 🔥 新增：狀態
    }
}