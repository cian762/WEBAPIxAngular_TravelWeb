namespace TravelWeb_API.DTO.ActivityDTO
{
    public class ActivityTicketInfoResponseDTO
    {
        // 商品介面要用的資料
        public required string ProductCode { get; set; }
        public string? ProductName { get; set; }
        public string? Description { get; set; }
        public string? TermsOfService { get; set; }
        public string? CoverImageUrl { get; set; }
        public string? Notes { get; set; }
        public int TicketCategoryId { get; set; }

        public DateOnly? StartDate { get; set; }
        public DateOnly? ExpiryDate { get; set; }
        public int? CurrentPrice { get; set; }
        
        // 待加入的欄位
        public int TicketStock { get; set; }
    }
}
