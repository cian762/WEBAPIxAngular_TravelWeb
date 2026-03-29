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
        public List<string>? PostPhoto { get; set; }
        public byte Status { get; set; }
        public string? AvatarUrl { get; set; }
        public string? AuthorName { get; set; }
        public string AuthorID { get; set; } = null!;
        public int? RegionId { get; set; }
        public string? RegionName { get; set; }
        public int CommentCount { get; set; }
        public int LikeCount { get; set; }
        public bool isLike { get; set; }       


    }
}
