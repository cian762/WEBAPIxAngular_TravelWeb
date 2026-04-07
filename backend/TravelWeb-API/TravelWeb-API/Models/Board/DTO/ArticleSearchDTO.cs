namespace TravelWeb_API.Models.Board.DTO
{
    public class ArticleSearchDTO
    {       
        public string? Keyword { get; set; }

        public string? authorKeyword { get; set; }
        public string? StartTime { get; set; }
        public string? EndTime { get; set; }
        public int? RegionId { get; set; }
        public List<int>? TagIds { get; set; }

    }
}
