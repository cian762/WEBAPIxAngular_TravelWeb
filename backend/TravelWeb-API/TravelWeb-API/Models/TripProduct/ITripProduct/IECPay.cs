namespace TravelWeb_API.Models.TripProduct.ITripProduct
{
    public interface IECPay
    {

        // 傳入 Order 物件，回傳 HTML 字串
        string GetPaymentForm(Order order);

        // 驗證回傳的檢查碼
        bool ValidateCheckMacValue(Dictionary<string, string> formData);
    }
}
