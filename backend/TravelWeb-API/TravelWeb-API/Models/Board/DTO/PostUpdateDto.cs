namespace TravelWeb_API.Models.Board.DTO
{
    public class PostUpdateDto
    {
        public int id { get; set; }
        public string? Title { get; set; }
        public string? PhotoUrl { get; set; }
        public byte Status { get; set; }
        public string? content { get; set; }
        public int? regionId { get; set; }
        public List<string>? photoUrlList { get; set; }

    }
  

}
