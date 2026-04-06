namespace TravelWeb_API.Models.Board.DTO
{
    public class AuthorInfo
    {      
        public string? avatarUrl { get; set; }
        public string? authorName { get; set; }
        public bool isCurrentUser { get; set; }        
        public int ArticleCount { get; set; }
    }
}
