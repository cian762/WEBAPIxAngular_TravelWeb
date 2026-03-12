namespace TravelWeb_API.Models.Board.DTO
{
    public class CommentsDTO
    {
        public string? AuthorName { get; set; }
        public string? AvatarUrl { get; set; }
        public string? Contents { get; set; }
        public DateTime CreatedAt { get; set; }
        public int LikeCount { get; set; }

        public bool isLiked { get; set; }

    }
}
