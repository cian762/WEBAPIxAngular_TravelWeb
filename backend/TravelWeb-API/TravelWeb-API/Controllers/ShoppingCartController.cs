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
        private readonly string _memberId;

        public ShoppingCartController(IShoppingCart cart, string memderId) 
        {
         _cart = cart;
          _memberId = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value!;

        }
        // 🏆 核心邏輯：判斷目前身分
     
        //下單完成後一次性刪除該使用者（MemberId）在購物車裡的所有商品
        [HttpDelete("clear")]
        public async Task<IActionResult> ClearCart([FromQuery] string? guestId)
        {
      
            await _cart.ClearCartAsync(_memberId);
            return Ok(new { message = "購物車已清空" });
        }
        //呼叫該會員購物車商品
        // 1. 取得購物車：GET api/ShoppingCart/{memberId}
        [HttpGet]
        public async Task<IActionResult> GetCart([FromQuery] string? guestId)
        {
            try
            {
                var id = _memberId;
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
            dto.MemberId = _memberId;
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
        public async Task<IActionResult> UpdateQuantity([FromQuery] string? guestId, [FromBody] UpdateCartQtyDTO dto)
        {
            try
            {
                // 這裡要把 URL 的 memberId 傳給 Service
                var id = _memberId;
                await _cart.UpdateQuantityAsync(dto, id);
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
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                         ?? User.Identity?.Name
                         ?? guestId;
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
            var memberId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.Identity?.Name;

            if (string.IsNullOrEmpty(memberId)) return Unauthorized();
            if (dtos == null || !dtos.Any()) return Ok(); // 沒東西要同步就直接回傳 OK

            await _cart.SyncCartAsync(dtos, memberId);
            return Ok(new { message = "同步完成" });
        }
        //遊客轉會員購物車搬遷這支目前先不用
        [HttpPost("migrate")]
        public async Task<IActionResult> Migrate([FromBody] MigrateCartDto dto)
        {
            try
            {
                await _cart.MigrateCartAsync(dto.GuestId, dto.MemberId);
                return Ok(new { success = true, message = "購物車合併成功" });
            }
            catch (Exception ex)
            {
                // 如果 Service 丟出「無效遊客」或「帳號不存在」，這裡會抓到
                return BadRequest(new { success = false, message = ex.Message });
            }
        }


    }
}
