namespace TravelWeb_API.DTO.ActivityDTO
{
    public class QrCodeValidationResponesDTO
    {

        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;

        public int? QrcodeId { get; set; }
        public string? QrToken { get; set; }
        public string? Status {  get; set; }
        public DateOnly? ExpiredDate { get; set; }
        public DateTime? UseAt { get; set; }

        public string ProductName { get; set; } = string.Empty;
        public string ProductCode { get; set; } = string.Empty;

        public byte[]? QRCodeImage { get; set; }
    }
}
