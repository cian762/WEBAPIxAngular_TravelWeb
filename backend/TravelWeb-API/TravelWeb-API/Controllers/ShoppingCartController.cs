using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
        //清空購物車
        [HttpDelete("Clear/{memberId}")]
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
        [HttpPatch("update-quantity")]
        public async Task<IActionResult> UpdateQuantity([FromBody] UpdateCartQtyDTO dto)
        {
            try
            {
                await _cart.UpdateQuantityAsync(dto);
                return Ok(new { success = true, message = "數量已更新" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }


    }
}
