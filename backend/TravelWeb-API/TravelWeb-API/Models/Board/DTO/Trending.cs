namespace TravelWeb_API.Models.Board.DTO
{
    public class Trending
    {
        public int articleId { get; set; }
        public int Type { get; set; }
        public string? title { get; set; }       
        public string? photoUrl { get; set; }
        public string? author { get; set; }
        public string? CreatedAt { get; set; }
    }
}
