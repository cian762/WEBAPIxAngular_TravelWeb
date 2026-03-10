using TravelWeb_API.Models.ActivityModel;

namespace TravelWeb_API.DTO.ActivityDTO
{
    public class FilterActivityDTO
    {
        public string? ActivityType { get; set; }
        public string? LaunchRegion { get; set; }
        public DateOnly? StartTime { get; set; }
        public DateOnly? EndTime { get; set; }
    }
}
