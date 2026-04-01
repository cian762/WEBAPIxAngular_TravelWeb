namespace TravelWeb_API.DTO.ActivityDTO
{
    public class ReviewResponseDTO
    {
        public int ReviewId { get; set; }

        public string MemberId { get; set; } = null!;

        public string MemberAvatar { get; set; } = string.Empty;

        public string Title { get; set; } = null!;

        public string? Comment { get; set; }

        public int Rating { get; set; }

        public DateOnly CreateDate { get; set; }

        public List<string?> ReviewImages { get; set; } = new List<string?>();
    }
}
