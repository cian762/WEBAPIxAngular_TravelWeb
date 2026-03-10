using Microsoft.AspNetCore.Mvc;
using TravelWeb_API.Models.Itinerary.DTO;
using TravelWeb_API.Models.Itinerary.Service;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace TravelWeb_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ItineraryController : ControllerBase
    {
        private readonly IItineraryservice _itineraryService;
        public ItineraryController(IItineraryservice itineraryService)
        {
            _itineraryService = itineraryService;
        }
        // POST 新增行程
        [HttpPost]
        public async Task<IActionResult> CreateItinerary([FromBody] ItineraryCreateDto dto)
        {
            // 1. 基本驗證：確保 DTO 不是空的
            if (dto == null)
            {
                return BadRequest("請求資料不可為空");
            }

            // 2. 業務驗證：例如檢查日期
            if (dto.StartTime > dto.EndTime)
            {
                return BadRequest("開始時間不能晚於結束時間");
            }

            try
            {
                // 3. 呼叫你剛寫好的 Service 邏輯
                int newId = await _itineraryService.CreateItineraryWithItemsAsync(dto);

                // 4. 回傳 201 Created，並在 Header 附上查詢該行程的 URL
                // (假設你有一個 GetById 的 Action)
                return CreatedAtAction(null, new { id = newId }, new { id = newId, message = "行程建立成功" });
            }
            catch (Exception ex)
            {
                // 5. 錯誤處理：實務上建議記錄 Log
                // _logger.LogError(ex, "建立行程時發生錯誤");
                return StatusCode(500, $"伺服器內部錯誤: {ex.Message}");
            }
        }

        // PUT api/<ItineraryController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<ItineraryController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
