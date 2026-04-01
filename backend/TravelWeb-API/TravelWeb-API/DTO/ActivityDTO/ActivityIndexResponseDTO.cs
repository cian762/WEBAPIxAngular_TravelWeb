namespace TravelWeb_API.DTO.ActivityDTO
{
    public class ActivityIndexResponseDTO
    {
        public int ActivityId { get; set; }
        public string? Title { get; set; }
        public IEnumerable<string>? Region { get; set; }
        public string? CoverImageUrl { get; set; }
        public int ViewCount { get; set; }
        public float AverageRating { get; set; }
    }
}
