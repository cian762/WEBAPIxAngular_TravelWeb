namespace TravelWeb_API.Models.Board.DTO
{
    public class JournalUpdateDTO
    {
        public string? Title { get; set; }
        public string? Cover { get; set; }        
        public List<JournalElementDTO>? Elements { get; set; }        
        public byte Status { get; set; }        
        public int? RegionId { get; set; }        
        
    }
}
