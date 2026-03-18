using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TravelWeb_API.Models.TripProduct;
using TravelWeb_API.Models.TripProduct.ITripProduct;

namespace TravelWeb_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly IECPay _ecpayService;
        private readonly IOrder _orderService;
        private readonly TripDbContext _tripDbContext;
        public PaymentController(IECPay ecpayService, IOrder orderService, TripDbContext tripDbContext)
        {
            _ecpayService = ecpayService;
            _orderService = orderService;
            _tripDbContext = tripDbContext;
        }
        //呼叫綠界付款畫面
        [HttpPost("Checkout/{orderId}")]
        public IActionResult Checkout(int orderId)
        {
            // 從資料庫抓取訂單資料
            var order = _tripDbContext.Orders.Include(o => o.OrderItems).FirstOrDefault(o => o.OrderId == orderId);
            if (order == null) return NotFound("訂單不存在");

            // 產生綠界表單 HTML
            string htmlForm = _ecpayService.GetPaymentForm(order);

            // 返回 Content 並指定為 text/html，瀏覽器收到會立刻自動 Submit 跳轉到綠界
            return Content(htmlForm, "text/html", System.Text.Encoding.UTF8);
        }
        //綠界回傳
        [HttpPost("EcpayReturn")]
        [Consumes("application/x-www-form-urlencoded")]
        public async Task<IActionResult> EcpayReturn([FromForm] Dictionary<string, string> formData)
        {
            // 1. 驗證 CheckMacValue
            if (!_ecpayService.ValidateCheckMacValue(formData))
            {
                // 如果失敗，會噴 400，這就是你之前在 ngrok 看到的錯誤
                return BadRequest("CheckMacValue 驗證失敗");
            }

            // 2. 判斷付款狀態 (RtnCode == 1 代表成功)
            if (formData["RtnCode"] == "1")
            {
                string merchantTradeNo = formData["MerchantTradeNo"];

                // 解析訂單 ID (照你原本的邏輯)
                string idPart = merchantTradeNo.Replace("TW", "").Split('T')[0];
                if (int.TryParse(idPart, out int orderId))
                {
                    var order = await _tripDbContext.Orders.FirstOrDefaultAsync(o => o.OrderId == orderId);

                    if (order != null && order.PaymentStatus != "已付款")
                    {
                        // 更新訂單
                        order.PaymentStatus = "已付款";
                        order.OrderStatus = "已處理";

                        // 更新交易紀錄
                        var transaction = await _tripDbContext.PaymentTransactions
                            .FirstOrDefaultAsync(t => t.OrderId == orderId && t.TransactionStatus == "待處理");

                        if (transaction != null)
                        {
                            transaction.ProviderTransactionNo = formData["TradeNo"]; // 綠界的交易序號
                            transaction.TransactionStatus = "成功";
                            transaction.CompletedAt = DateTime.Now;
                        }

                        await _tripDbContext.SaveChangesAsync();
                    }
                }
            }

            // 3. 💡 成功後必須回傳 1|OK，否則綠界會一直重發通知
            return Content("1|OK");
        }
    }

}
