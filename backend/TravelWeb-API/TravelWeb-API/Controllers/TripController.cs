using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TravelWeb_API.Models.TripProduct.ITripProduct;

namespace TravelWeb_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TripController : ControllerBase
    {
      private readonly ITripproductTable _tripproduct;

        public TripController(ITripproductTable tripproduct) 
        {
          _tripproduct = tripproduct;
        }
        //呼叫商品主表
        [HttpGet("All")]
        public async Task<IActionResult>Get()
        {
            var products = await _tripproduct.GetAllAsync();

            // 回傳 200 OK 與資料
            return Ok(products);
        }






    }
}
