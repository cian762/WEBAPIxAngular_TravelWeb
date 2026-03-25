using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using TravelWeb_API.Models.TripProduct;
using TravelWeb_API.Models.TripProduct.ITripProduct;
using TravelWeb_API.Models.TripProduct.STripProduct;
using TravelWeb_API.Models.TripProduct.TripDTO;

namespace TravelWeb_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class OrderController : ControllerBase
    {
        private readonly IOrder _order;
        private readonly IECPay _ecpay;
        public OrderController(IOrder order, IECPay ecpay) {
            _order = order;
            _ecpay = ecpay;
          
        }
        private string? CurrentMemberId =>
          User.FindFirst("MemberId")?.Value ?? // 👈 優先抓你自定義的這個標籤
          User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value ??
          User.Identity?.Name;

        // 取得當前登入者 ID 的輔助方法
        [HttpPut("cancel/{orderId}")]
        public async Task<IActionResult> CancelOrder(int orderId)
        {
            var memberId = CurrentMemberId; // 👈 從 Token 抓 ID
            if (string.IsNullOrEmpty(memberId)) return Unauthorized();

            // 2. 呼叫 Service 執行取消邏輯 (包含狀態機檢查)
            var result = await _order.CancelOrderAsync(orderId, memberId);

            if (!result)
            {
                // 如果失敗，可能是訂單不存在、不屬於該用戶，或是狀態不允許取消
                return BadRequest("取消失敗：訂單可能已完成、已出票或無此權限。");
            }

            return Ok(new { Message = "訂單已成功取消" });
        }
        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto dto)
        {
            var memberId = CurrentMemberId;
            Console.WriteLine($"目前登入的會員 ID 是: {memberId}");
            if (string.IsNullOrEmpty(memberId)) return Unauthorized();
            try
            {
                if (string.IsNullOrEmpty(memberId))
                {
                    return Unauthorized("找不到會員資訊");
                }

                // 1. 呼叫 Service 建立訂單 (現在回傳的是整個 Order 物件)
                var order = await _order.CreateOrderAsync(dto, memberId);

                // 2. 呼叫 SECPay 服務產生綠界 HTML
                // 這會用到 order 裡的 OrderId 和 TotalAmount
                string paymentForm = _ecpay.GetPaymentForm(order);

                // 3. 回傳包含金流表單的物件
                return Ok(new
                {
                    Message = "訂單建立成功，準備跳轉支付",
                    OrderId = order.OrderId,
                    PaymentForm = paymentForm // 這串給前端執行
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }
        [HttpGet("my-orders")]
        public async Task<IActionResult> GetMyOrders()
        {
            var memberId = CurrentMemberId;
            if (string.IsNullOrEmpty(memberId)) return Unauthorized();
            try
            {
                // 1. 檢查 MemberId 是否有傳入
                if (string.IsNullOrEmpty(memberId))
                {
                    return BadRequest(new { Message = "請提供會員編號" });
                }

                // 2. 呼叫 Service 取得該會員的所有訂單
                var orders = await _order.GetMemberOrdersAsync(memberId);

                // 3. 判斷是否有資料
                if (orders == null || !orders.Any())
                {
                    // 回傳 200 OK 但內容是空陣列，這對前端處理列表比較方便
                    return Ok(new List<OrderListDto>());
                }

                return Ok(orders);
            }
            catch (Exception ex)
            {
                // 捕捉 Service 層丟出的錯誤
                return BadRequest(new { Message = "查詢訂單失敗: " + ex.Message });
            }
        }
        [HttpGet("{orderId}")]
        public async Task<IActionResult> GetOrderDetail(int orderId)
        {
            var memberId = CurrentMemberId;
            if (string.IsNullOrEmpty(memberId)) return Unauthorized();
            // 呼叫你剛寫好的服務層
            var detail = await _order.GetOrderDetailAsync(orderId, memberId);

            if (detail == null)
            {
                return NotFound(new { Message = "找不到該筆訂單，或您無權限存取。" });
            }

            return Ok(detail);
        }
        [HttpPost("preview")]
        public async Task<IActionResult> GetPreview([FromBody] CreateOrderDto dto)
        {
            var memberId = CurrentMemberId;
            if (string.IsNullOrEmpty(memberId)) return Unauthorized();
            try
            {
                // 1. 驗證基本輸入
                if (dto == null)
                {
                    return BadRequest(new { Message = "請提供預覽資訊" });
                }

                if (string.IsNullOrEmpty(memberId))
                {
                    return BadRequest(new { Message = "會員編號不可為空" });
                }

                // 2. 呼叫 Service 取得模擬的訂單詳情 (不存檔)
                var previewData = await _order.GetCheckoutPreviewAsync(dto, memberId);

                // 3. 回傳 200 OK 以及計算好的明細
                return Ok(previewData);
            }
            catch (Exception ex)
            {
                // 捕捉 Service 丟出的錯誤（例如：名額不足、找不到商品等）
                return BadRequest(new { Message = "預覽失敗: " + ex.Message });
            }
        }
    }



}

