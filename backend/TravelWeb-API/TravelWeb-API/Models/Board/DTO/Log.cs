namespace TravelWeb_API.Models.Board.DTO
{
    public class Log
    {
        public int TargetID { get; set; }
        public int ViolationType { get; set; }
        public string? Reason { get; set; }
        public string? Photo { get; set; }

    }
}
