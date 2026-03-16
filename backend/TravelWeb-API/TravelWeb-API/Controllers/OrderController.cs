using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TravelWeb_API.Models.TripProduct;
using TravelWeb_API.Models.TripProduct.ITripProduct;
using TravelWeb_API.Models.TripProduct.TripDTO;

namespace TravelWeb_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly IOrder _order;
        public OrderController(IOrder order) {
            _order = order;

        }

        [HttpPut("cancel/{orderId}/{memberId}")]
        public async Task<IActionResult> CancelOrder(int orderId, string memberId)
        {
            // 1. 從 Token 的 Claims 中提取 MemberId (對應你在產生 Token 時設定的 ClaimType)
            // 通常是 ClaimTypes.NameIdentifier 或 "sub"
            //var memberId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            //if (string.IsNullOrEmpty(memberId))
            //{
            //    return Unauthorized("無法取得用戶資訊");
            //}

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
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto dto, [FromQuery] string memberId)
        {
            try
            {
                // 1. 從 Token 中抓取 MemberId (這是最安全的做法)
                // 假設你的 Claim 類型是 NameIdentifier
                //var memberId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(memberId))
                {
                    return Unauthorized("找不到會員資訊");
                }

                // 2. 呼叫 Service
                int orderId = await _order.CreateOrderAsync(dto, memberId);

                // 3. 回傳 201 Created 並告知訂單 ID
                return Ok(new { Message = "訂單建立成功", OrderId = orderId });
            }
            catch (Exception ex)
            {
                // 這裡可以記錄 log
                return BadRequest(new { Message = ex.Message });
            }
        }
        [HttpGet("my-orders")]
        public async Task<IActionResult> GetMyOrders([FromQuery] string memberId)
        {
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
        public async Task<IActionResult> GetOrderDetail(int orderId, [FromQuery] string memberId)
        {
            // 呼叫你剛寫好的服務層
            var detail = await _order.GetOrderDetailAsync(orderId, memberId);

            if (detail == null)
            {
                return NotFound(new { Message = "找不到該筆訂單，或您無權限存取。" });
            }

            return Ok(detail);
        }
        [HttpPost("preview")]
        public async Task<IActionResult> GetPreview([FromBody] CreateOrderDto dto, [FromQuery] string memberId)
        {
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

