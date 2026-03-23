namespace TravelWeb_API.DTO.ActivityDTO
{
    public class ActivityTicketIntroResponseDTO
    {
        public required string ProductCode { get; set; }
        public required string ProductName { get; set; }
        public int? CurrentPrice { get; set; }
        public string? Notes { get; set; }
        public string? CoverImageUrl { get; set; }
        public string? Status { get; set; }
    }
}
