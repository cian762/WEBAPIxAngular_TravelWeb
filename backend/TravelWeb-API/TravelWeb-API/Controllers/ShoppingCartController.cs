using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TravelWeb_API.Models.TripProduct.ITripProduct;
using TravelWeb_API.Models.TripProduct.TripDTO;

namespace TravelWeb_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ShoppingCartController : ControllerBase
    {
        private readonly IShoppingCart _cart;

        public ShoppingCartController(IShoppingCart cart) 
        {
         _cart = cart;
        }
        //下單完成後一次性刪除該使用者（MemberId）在購物車裡的所有商品
        [HttpDelete("clear/{memberId}")]
        public async Task<IActionResult> ClearCart(string memberId)
        {
            await _cart.ClearCartAsync(memberId);
            return Ok(new { message = "購物車已清空" });
        }
        //呼叫該會員購物車商品
        // 1. 取得購物車：GET api/ShoppingCart/{memberId}
        [HttpGet("{memberId}")]
        public async Task<IActionResult> GetCart(string memberId)
        {
            try
            {
                var cart = await _cart.GetCartAsync(memberId);
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
        [HttpPatch("update-quantity/{memberId}")]
        public async Task<IActionResult> UpdateQuantity(string memberId, [FromBody] UpdateCartQtyDTO dto)
        {
            try
            {
                // 這裡要把 URL 的 memberId 傳給 Service
                await _cart.UpdateQuantityAsync(dto, memberId);
                return Ok(new { success = true, message = "數量已更新" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        //刪除單筆及多筆購物車API可自選刪除
        // 刪除項目：同樣建議從網址或 DTO 帶入 memberId
        [HttpPost("remove-items/{memberId}")]
        public async Task<IActionResult> RemoveItems(string memberId, [FromBody] DeleteCartItemsDTO dto)
        {
            if (dto.CartIds == null || !dto.CartIds.Any()) return BadRequest("未提供 ID");

            try
            {
                await _cart.RemoveItemsAsync(dto.CartIds, memberId);
                return Ok(new { message = "刪除成功" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"錯誤: {ex.Message}");
            }
        }
        //遊客轉會員購物車搬遷
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
