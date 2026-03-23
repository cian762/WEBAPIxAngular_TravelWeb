namespace TravelWeb_API.DTO.ActivityDTO
{
    public class RouteResponseDTO
    {
        public string DistanceText { get; set; } = "";
        public string DurationText { get; set; } = "";
        public int DistanceMeters { get; set; }
        public string EncodedPolyline { get; set; } = "";
        public List<RouteOptionDTO> Routes { get; set; } = new();
    }

}
