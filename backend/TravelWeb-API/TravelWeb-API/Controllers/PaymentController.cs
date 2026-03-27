using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TravelWeb_API.Models.TripProduct;
using TravelWeb_API.Models.TripProduct.ITripProduct;
using TravelWeb_API.Services;

namespace TravelWeb_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly IECPay _ecpayService;
        private readonly IOrder _orderService;
        private readonly TripDbContext _tripDbContext;
        private readonly TicketService _ticketService;
        private readonly EmailService _emailService;
        public PaymentController(IECPay ecpayService, IOrder orderService, TripDbContext tripDbContext, TicketService ticketService, EmailService emailService)
        {
            _ecpayService = ecpayService;
            _orderService = orderService;
            _tripDbContext = tripDbContext;

            //20260326 陳冠甫加入QRCode 功能所需注入服務
            _ticketService = ticketService;
            _emailService = emailService;
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
        [HttpPost("PaymentCallback")]
        public IActionResult PaymentCallback()
        {
            // 這裡不做邏輯處理，單純把使用者轉回前端
            return Redirect("http://localhost:4200/");
        }
        //綠界回傳
        [HttpPost("EcpayReturn")]
        [Consumes("application/x-www-form-urlencoded")]
        public async Task<IActionResult> EcpayReturn([FromForm] Dictionary<string, string> formData)
        {
            // 1. 驗證 CheckMacValue (這步過了才動資料庫)
            if (!_ecpayService.ValidateCheckMacValue(formData))
            {
                return BadRequest("CheckMacValue 驗證失敗");
            }

            // 2. 判斷付款狀態 (RtnCode == "1" 代表付款成功)
            if (formData["RtnCode"] == "1")
            {
                // 從 CustomField1 拿到你的 OrderId
                if (formData.TryGetValue("CustomField1", out var customField1) && int.TryParse(customField1, out int orderId))
                {
                    // 💡 一次把訂單跟交易紀錄都抓出來
                    var order = await _tripDbContext.Orders.FirstOrDefaultAsync(o => o.OrderId == orderId);
                    var transaction = await _tripDbContext.PaymentTransactions
                        .FirstOrDefaultAsync(t => t.OrderId == orderId);

                    if (order != null)
                    {
                        // A. 更新訂單主表狀態
                        order.PaymentStatus = "已付款";
                        order.OrderStatus = "已處理"; // 或是你定義的下一步狀態

                        // B. 更新交易明細表 (把綠界回傳的空值填進去)
                        if (transaction != null)
                        {
                            // 填入綠界的交易編號 (例如: 2603251634329768)
                            transaction.ProviderTransactionNo = formData.ContainsKey("TradeNo") ? formData["TradeNo"] : "";

                            // 填入付款方式 (例如: Credit_CreditCard)
                            transaction.PaymentMethod = formData.ContainsKey("PaymentType") ? formData["PaymentType"] : "ECPay";

                            // 填入實際付款金額 (選填)
                            // transaction.Amount = decimal.Parse(formData["TradeAmt"]);

                            // 狀態改為成功
                            transaction.TransactionStatus = "成功";

                            // 填入完成時間 (優先使用綠界回傳的 PaymentDate)
                            if (formData.TryGetValue("PaymentDate", out var payDate))
                            {
                                transaction.CompletedAt = DateTime.Parse(payDate);
                            }
                            else
                            {
                                transaction.CompletedAt = DateTime.Now;
                            }
                        }


                        await _tripDbContext.SaveChangesAsync();

                        //QRcode 送信放這
                        //根據內容物生成 QRCode，每個商品數量代表一個 QRcode
                        await _ticketService.CreateQrCodeForOrderAsync(orderId);

                        //將該訂單所屬 QRcode 打包用信件方式寄出
                        await _emailService.SendOrderTicketEmailAsync(orderId);
                    }
                }
            }

            // 3. 嚴格遵守綠界規範：回傳純文字 1|OK (不然綠界會一直重傳)
            return Content("1|OK");
        }

    }
}
