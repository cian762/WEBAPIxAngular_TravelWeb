namespace TravelWeb_API.DTO.ActivityDTO
{
    public class ActivityInfoReponseDTO
    {
        public int ActivityId { get; set; }
        public string? Title { get; set; }
        public IEnumerable<string?>? Type { get; set; }
        public IEnumerable<string>? Region { get; set; }
        public DateOnly? Start { get; set; }
        public DateOnly? End { get; set; }
    }
}
