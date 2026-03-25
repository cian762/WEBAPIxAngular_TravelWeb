namespace TravelWeb_API.Models.Board.DTO
{
    public class ArticleDataDTO
    {
        public int articleId { get; set; }
        public string? title { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? photoUrl { get; set; }
        public string userID { get; set; } = null!;
        public string? userName { get; set; }
        public string? userAvatar { get; set; }
        public List<TagDTO>? tags { get; set; }
    }

    public class TagDTO
    {
        public int TagId { get; set; }
        public string TagName { get; set; } = null!;
    }
}

