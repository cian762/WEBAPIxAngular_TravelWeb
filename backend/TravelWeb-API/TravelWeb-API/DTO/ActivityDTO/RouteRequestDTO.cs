namespace TravelWeb_API.DTO.ActivityDTO
{
    public class RouteRequestDTO
    {
        public double OriginLat { get; set; }
        public double OriginLng { get; set; }
        public double DestinationLat { get; set; }
        public double DestinationLng { get; set; }
        public string TravelMode { get; set; } = "DRIVE";
    }
}
