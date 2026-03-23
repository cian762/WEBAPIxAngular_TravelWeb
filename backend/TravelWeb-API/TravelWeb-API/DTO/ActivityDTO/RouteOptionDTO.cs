namespace TravelWeb_API.DTO.ActivityDTO
{
    public class RouteOptionDTO
    {
        public string Summary { get; set; } = "";
        public string DistanceText { get; set; } = "";
        public string DurationText { get; set; } = "";
        public int DistanceMeters { get; set; }
        public string EncodedPolyline { get; set; } = "";
        public List<string> RouteLabels { get; set; } = new();
        public List<RouteStepDTO> Steps { get; set; } = new();
    }
}
