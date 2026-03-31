using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using TravelWeb_API.Models.TripProduct.ITripProduct;
using TravelWeb_API.Models.TripProduct.TripDTO;

namespace TravelWeb_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // 🔒 既然都要同步到會員帳號了，一定要有 Token (Cookie)
    public class ShoppingCartController : ControllerBase
    {
        private readonly IShoppingCart _cart;
      

        public ShoppingCartController(IShoppingCart cart) 
        {
         _cart = cart;
        }
        private string? CurrentMemberId =>
         User.FindFirst("MemberId")?.Value ?? // 👈 優先抓你自定義的這個標籤
         User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value ??
         User.Identity?.Name;
        // 🏆 核心邏輯：判斷目前身分

        //下單完成後一次性刪除該使用者（MemberId）在購物車裡的所有商品
        [HttpDelete("clear")]
        public async Task<IActionResult> ClearCart([FromQuery] string? guestId)
        {
      
            await _cart.ClearCartAsync(CurrentMemberId!);
            return Ok(new { message = "購物車已清空" });
        }
        //呼叫該會員購物車商品
        // 1. 取得購物車：GET api/ShoppingCart/{memberId}
        [HttpGet]
        public async Task<IActionResult> GetCart()
        {
            try
            {
                var id = CurrentMemberId;
                if (string.IsNullOrEmpty(id)) return BadRequest("無法辨識用戶身分");

                var cart = await _cart.GetCartAsync(id);
                return Ok(cart);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "讀取購物車失敗", detail = ex.Message });
            }
        }

        // 2. 加入購物車：POST api/ShoppingCart/add
        [HttpPost("add")]
        public async Task<IActionResult> AddToCart([FromBody] AddToCartDTO dto)
        {
            dto.MemberId = CurrentMemberId;
            if (dto == null) return BadRequest("資料不能為空");

            try
            {
                await _cart.AddToCartAsync(dto);
                return Ok(new { message = "成功加入購物車" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "加入失敗", detail = ex.Message });
            }
        }
        //跟新購物車數量
        [HttpPatch("update-quantity")]
        public async Task<IActionResult> UpdateQuantity([FromBody] UpdateCartQtyDTO dto)
        {
            try
            {
                // 這裡要把 URL 的 memberId 傳給 Service
                var id = CurrentMemberId;
                await _cart.UpdateQuantityAsync(dto, id!);
                return Ok(new { success = true, message = "數量已更新" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        //刪除單筆及多筆購物車API可自選刪除
        // 刪除項目：同樣建議從網址或 DTO 帶入 memberId
        [HttpPost("remove-items")]
        public async Task<IActionResult> RemoveItems([FromBody] DeleteCartItemsDTO dto, [FromQuery] string? guestId)
        {
            // 1. 決定最終要使用的 ID (優先用 Token，沒有才用傳進來的 guestId)
            var userId = CurrentMemberId;
            if (dto.CartIds == null || !dto.CartIds.Any()) return BadRequest("未提供 ID");

            try
            {
                await _cart.RemoveItemsAsync(dto.CartIds, userId!);
                return Ok(new { message = "刪除成功" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"錯誤: {ex.Message}");
            }
        }
        //新的處理遊客搬遷到會員
        [HttpPost("sync")]
        public async Task<IActionResult> SyncCart([FromBody] List<AddToCartDTO> dtos)
        {
            // 從 Cookie 拿 ID
            var memberId = CurrentMemberId;

            if (string.IsNullOrEmpty(memberId)) return Unauthorized();
            if (dtos == null || !dtos.Any()) return Ok(); // 沒東西要同步就直接回傳 OK

            await _cart.SyncCartAsync(dtos, memberId);
            return Ok(new { message = "同步完成" });
        }

    }
}
