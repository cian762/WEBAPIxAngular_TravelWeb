namespace TravelWeb_API.Models.Itinerary.DTO
{
    public class ErrorReportDto
    {
        public int? ItineraryId { get; set; }
        public int? VersionId { get; set; }
        public string ErrorType { get; set; }
        public string SeverityLevel { get; set; }
        public string ErrorMessage { get; set; }
        public string ErrorReason { get; set; }
        public int? RelatedItemId { get; set; }
        public bool? IsConfirmed { get; set; }
    }
}
