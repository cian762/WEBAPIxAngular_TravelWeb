namespace TravelWeb_API.DTO.ActivityDTO
{
    public class ActivityInfoResponseDTO
    {
        public int ActivityId { get; set; }

        public string? Title { get; set; }

        public List<string>? Regions { get; set; }

        public List<string>? Types { get; set; }

        public string? Description { get; set; }

        public DateOnly? StartTime { get; set; }

        public DateOnly? EndTime { get; set; }

        public string? Address { get; set; }

        public decimal? Longitude { get; set; }

        public decimal? Latitude { get; set; }

        public string? Propaganda { get; set; }

        public string? OfficialLink { get; set; }

        public List<string?> Images { get; set; } = new List<string?>();

    }
}
