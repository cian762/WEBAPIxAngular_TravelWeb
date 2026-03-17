namespace TravelWeb_API.Models.TripProduct.TripDTO
{
    public class ECPaySetting
    {
        public string MerchantID { get; set; } = string.Empty;
        public string HashKey { get; set; } = string.Empty;
        public string HashIV { get; set; } = string.Empty;
        public string PaymentUrl { get; set; } = string.Empty;
        public string ReturnURL { get; set; } = string.Empty;
        public string ClientBackURL { get; set; } = string.Empty;
    }
}
