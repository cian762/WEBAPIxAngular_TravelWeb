using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TravelWeb_API.Models.TripProduct.ITripProduct;
using TravelWeb_API.Models.TripProduct.TripDTO;

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

        // 2. 重點：【搜尋與分頁】 API
        // 使用 [FromQuery] 讓前端可以用 ?Keyword=日本&Page=1 這種方式傳參
        [HttpGet("Search")]
        public async Task<IActionResult> Search([FromQuery] ProductQueryDTO queryDto)
        {
            try
            {
                // 呼叫你在 Repository 寫好的搜尋分頁邏輯
                var result = await _tripproduct.SearchProductsAsync(queryDto);

                // result 會包含 TotalCount (總筆數) 和 Data (當頁資料)
                return Ok(result);
            }
            catch (Exception ex)
            {
                // 基礎錯誤處理
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // 3. 輔助 API：給前端畫選單用的地區與標籤
        [HttpGet("MetaData")]
        public async Task<IActionResult> GetMetaData()
        {
            var regions = await _tripproduct.GetRegionsAllAsync();
            var tags = await _tripproduct.GetTagsAllAsync();

            return Ok(new
            {
                Regions = regions,
                Tags = tags
            });
        }
        //=====================================================
        //這裡是商品詳細頁




    }
}
