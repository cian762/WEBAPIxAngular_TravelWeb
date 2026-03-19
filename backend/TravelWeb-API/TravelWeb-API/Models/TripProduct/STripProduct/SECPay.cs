using System.Net;
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
            string itemName = string.Join("|", order.OrderItems.Select(oi => oi.ProductNameSnapshot));
            if (itemName.Length > 200) itemName = itemName.Substring(0, 197) + "...";

            var parameters = new Dictionary<string, string>
            {
                { "MerchantID", ecpaySection["MerchantID"]! },
                { "MerchantTradeNo",$"TW{order.OrderId}{DateTime.Now:MMddHH}"},
                { "MerchantTradeDate", DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") },
                { "PaymentType", "aio" },
                { "TotalAmount", ((int)order.TotalAmount!).ToString() },
                { "TradeDesc", "TravelWeb" },
                { "ItemName", itemName },
                { "ReturnURL", ecpaySection["ReturnURL"]! },
                { "ClientBackURL", ecpaySection["ClientBackURL"]! },
                { "ChoosePayment", "Credit" },
                { "EncryptType", "1" }
            };

            parameters["CheckMacValue"] = GenerateCheckMacValue(parameters);
            return GenerateHtmlForm(ecpaySection["PaymentUrl"]!, parameters);
        }

        // 2. 驗證回傳的檢查碼
        // 驗證綠界回傳的 CheckMacValue
        public bool ValidateCheckMacValue(Dictionary<string, string> formData)
        {
            if (!formData.ContainsKey("CheckMacValue")) return false;

            string receivedValue = formData["CheckMacValue"];

            // 1. 複製一份資料並移除 CheckMacValue 本身，才不會參與雜湊計算
            var checkDict = new Dictionary<string, string>(formData);
            checkDict.Remove("CheckMacValue");

            // 2. 使用相同的邏輯算出雜湊值
            string calculatedValue = GenerateCheckMacValue(checkDict);

            // 3. 比對是否一致 (不區分大小寫較安全)
            return string.Equals(receivedValue, calculatedValue, StringComparison.OrdinalIgnoreCase);
        }
        // --- 以下為私有輔助方法 ---
        private string GenerateCheckMacValue(Dictionary<string, string> parameters)
        {
            var ecpaySection = _config.GetSection("ECPay");
            string hashKey = ecpaySection["HashKey"]!;
            string hashIV = ecpaySection["HashIV"]!;

            // 1. 字典排序並串接 (過濾掉 CheckMacValue 且確保沒有 Null)
            var sortedParams = parameters
                .Where(p => p.Key != "CheckMacValue" && !string.IsNullOrEmpty(p.Value))
                .OrderBy(p => p.Key)
                .Select(p => $"{p.Key}={p.Value}");

            string rawData = $"HashKey={hashKey}&{string.Join("&", sortedParams)}&HashIV={hashIV}";

            // 2. URL Encode 並處理特殊符號
            // 使用 WebUtility.UrlEncode 或 HttpUtility.UrlEncode 都可以，但後續取代要小心
            string encodedData = HttpUtility.UrlEncode(rawData).ToLower();

            // 綠界專屬取代規則
            encodedData = encodedData
                .Replace("%20", "+")
                .Replace("%2d", "-")
                .Replace("%5f", "_")
                .Replace("%2e", ".")
                .Replace("%21", "!")
                .Replace("%2a", "*")
                .Replace("%28", "(")
                .Replace("%29", ")");

            // 3. SHA256 加密
            using (var sha256 = SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(encodedData));
                // 直接用 BitConverter 或 StringBuilder 轉大寫
                return BitConverter.ToString(hashBytes).Replace("-", "").ToUpper();
            }
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
