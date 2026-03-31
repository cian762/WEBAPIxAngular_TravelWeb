using TravelWeb_API.Models.Board.DbSet;

namespace TravelWeb_API.Models.Board.DTO
{
    public class CommentsDTO
    {
        public int CommentId { get; set; }
        public string? AuthorName { get; set; }
        public string? AvatarUrl { get; set; }
        public string? Contents { get; set; }
        public DateTime CreatedAt { get; set; }
        public int LikeCount { get; set; }
        public bool isLiked { get; set; }
        public string? CommentPhoto { get; set; }
        public List<CommentsDTO>? ReplyComments { get; set; }

    }
}
