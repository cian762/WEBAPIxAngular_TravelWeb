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
            // 1. 驗證
            if (!_ecpayService.ValidateCheckMacValue(formData))
            {
                return BadRequest("CheckMacValue 驗證失敗");
            }

            // 2. 判斷付款成功
            if (formData["RtnCode"] == "1")
            {
                string merchantTradeNo = formData["MerchantTradeNo"]; // 例如: TW000002T143158

                // 較安全的拆法：拿掉前兩碼 TW，然後取到 T 之前的所有數字
                string idPart = merchantTradeNo.Substring(2).Split('T')[0];

                if (int.TryParse(idPart, out int orderId))
                {
                    // 更新 Orders
                    var order = await _tripDbContext.Orders.FirstOrDefaultAsync(o => o.OrderId == orderId);
                    if (order != null && order.PaymentStatus != "已付款") // 防止重複更新
                    {
                        order.PaymentStatus = "已付款";
                        order.OrderStatus = "已處理";

                        // 更新 Transaction
                        var transaction = await _tripDbContext.PaymentTransactions
                            .FirstOrDefaultAsync(t => t.OrderId == orderId && t.TransactionStatus == "待處理");

                        if (transaction != null)
                        {
                            transaction.ProviderTransactionNo = formData["TradeNo"];
                            transaction.TransactionStatus = "成功";
                            transaction.CompletedAt = DateTime.Now;
                        }

                        await _tripDbContext.SaveChangesAsync();
                    }
                }
            }

            return Content("1|OK");
        }
    }
}
