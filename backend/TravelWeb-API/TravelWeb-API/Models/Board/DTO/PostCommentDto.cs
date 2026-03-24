namespace TravelWeb_API.Models.Board.DTO
{
    public class PostCommentDto
    {        
        public int articleID { get; set; }        
        public string contents { get; set; } = null!;
        public int? parentID { get; set; }

    }
}
