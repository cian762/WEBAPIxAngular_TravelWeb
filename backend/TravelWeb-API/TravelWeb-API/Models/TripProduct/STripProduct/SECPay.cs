using System.Security.Cryptography;
using System.Text;
using System.Web;
using TravelWeb_API.Models.TripProduct.ITripProduct;

namespace TravelWeb_API.Models.TripProduct.STripProduct
{
    public class SECPay : IECPay
    {
        private readonly IConfiguration _config;
        public SECPay(IConfiguration config)
        {
            _config = config;
        }
        // 1. 產生送給綠界的 HTML 表單
        public string GetPaymentForm(Order order)
        {
            var ecpaySection = _config.GetSection("ECPay");

            // 組合商品名稱
            string itemName = string.Join("#", order.OrderItems.Select(oi => oi.ProductNameSnapshot));
            if (itemName.Length > 200) itemName = itemName.Substring(0, 197) + "...";

            var parameters = new Dictionary<string, string>
            {
                { "MerchantID", ecpaySection["MerchantID"]! },
                { "MerchantTradeNo", $"TW{order.OrderId:D6}T{DateTime.Now:HHmmss}" },
                { "MerchantTradeDate", DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") },
                { "PaymentType", "aio" },
                { "TotalAmount", ((int)order.TotalAmount!).ToString() },
                { "TradeDesc", "TravelWeb 訂單支付" },
                { "ItemName", itemName },
                { "ReturnURL", ecpaySection["ReturnURL"]! },
                { "OrderResultURL", ecpaySection["ClientBackURL"]! },
                { "ChoosePayment", "ALL" },
                { "EncryptType", "1" }
            };

            parameters["CheckMacValue"] = GenerateCheckMacValue(parameters);
            return GenerateHtmlForm(ecpaySection["PaymentUrl"]!, parameters);
        }

        // 2. 驗證回傳的檢查碼
        public bool ValidateCheckMacValue(Dictionary<string, string> formData)
        {
            var dict = new Dictionary<string, string>(formData);
            if (!dict.ContainsKey("CheckMacValue")) return false;

            string originalValue = dict["CheckMacValue"];
            dict.Remove("CheckMacValue");

            return originalValue == GenerateCheckMacValue(dict);
        }

        // --- 以下為私有輔助方法 ---

        private string GenerateCheckMacValue(Dictionary<string, string> parameters)
        {
            var ecpaySection = _config.GetSection("ECPay");
            string hashKey = ecpaySection["HashKey"]!;
            string hashIV = ecpaySection["HashIV"]!;

            var sortedParams = parameters.OrderBy(p => p.Key).Select(p => $"{p.Key}={p.Value}");
            string rawData = $"HashKey={hashKey}&{string.Join("&", sortedParams)}&HashIV={hashIV}";

            // 綠界特有的編碼取代規則
            string encodedData = HttpUtility.UrlEncode(rawData).ToLower();
            encodedData = encodedData.Replace("%2d", "-").Replace("%5f", "_").Replace("%2e", ".").Replace("%21", "!")
                                     .Replace("%2a", "*").Replace("%28", "(").Replace("%29", ")");

            using var sha256 = SHA256.Create();
            byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(encodedData));
            return BitConverter.ToString(hashBytes).Replace("-", "").ToUpper();
        }

        private string GenerateHtmlForm(string actionUrl, Dictionary<string, string> parameters)
        {
            var sb = new StringBuilder();
            sb.Append($@"<form id='ecpay-form' method='post' action='{actionUrl}'>");
            foreach (var kvp in parameters)
            {
                sb.Append($@"<input type='hidden' name='{kvp.Key}' value='{kvp.Value}' />");
            }
            sb.Append("</form>");
            sb.Append("<script>document.getElementById('ecpay-form').submit();</script>");
            return sb.ToString();
        }
    }

}
