namespace TravelWeb_API.Models.Board.DTO
{
    public class PostDetailDto
    {
        public string? Title { get; set; }
        public byte Type { get; set; }
        public string? Cover { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        
        public string? Contents { get; set; }
        public int? RegionId { get; set; }
        public List<string?>? PostPhoto;

        public string? AvatarUrl { get; set; }
        public string? AuthorName { get; set; }


    }
}
