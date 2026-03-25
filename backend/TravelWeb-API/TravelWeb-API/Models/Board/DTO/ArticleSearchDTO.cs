namespace TravelWeb_API.Models.Board.DTO
{
    public class ArticleSearchDTO
    {       
        public string? Keyword { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string? AuthorId { get; set; }
        public List<int>? TagIds { get; set; }

    }
}
