using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TravelWeb_API.Models.Itinerary.DTO;
using TravelWeb_API.Models.Itinerary.Service;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace TravelWeb_API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ItineraryController : ControllerBase
    {
        private readonly IItineraryservice _itineraryService;

        public ItineraryController(IItineraryservice itineraryService)
        {
            _itineraryService = itineraryService;

        }
        //GET透過行程ID取得行程資訊
        [HttpGet("{id}")]
        public async Task<IActionResult> GetbyItineraryId(int id)
        {
            if (id == 0)
            {
                return BadRequest("行程ID錯誤");
            }
            var result = await _itineraryService.GetItineraryDetailAsync(id);
            if (result == null)
            {
                return NotFound("找不到該行程");
            }
            return Ok(result);
        }
        //GET抓所有的歷史行程
        [HttpGet("{id}/history")]
        public async Task<IActionResult> GetHistory(int id)
        {
            var history = await _itineraryService.GetVersionHistoryAsync(id);
            return Ok(history);
        }
        // POST 新增行程
        [HttpPost]
        public async Task<IActionResult> CreateItinerary([FromBody] ItineraryCreateDto dto)
        {
            var memberId = User.FindFirst("MemberId")?.Value ?? "tw_user_001";
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
                // 3. 呼叫 Service
                int newId = await _itineraryService.CreateItineraryWithItemsAsync(dto, memberId);

                // 4. 回傳 201 Created，並在 Header 附上查詢該行程的 URL

                return CreatedAtAction("GetbyItineraryId", new { id = newId }, new { id = newId, message = "行程建立成功" });
            }
            catch (Exception ex)
            {
                // 5. 錯誤處理：實務上建議記錄 Log
                // _logger.LogError(ex, "建立行程時發生錯誤");
                return StatusCode(500, $"伺服器內部錯誤: {ex.Message}");
            }
        }
        //PATCH新增天數
        [HttpPatch("{id}/extend-day")]
        public async Task<IActionResult> ExtendItineraryDay(int id)
        {
            try
            {
                var result = await _itineraryService.ExtendOneDayAsync(id);
                return Ok(new { success = true, newEndTime = result });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        //POST修改行程，基於版本表與多個行程細項清單
        [HttpPost("{id}/save-snapshot")]
        public async Task<IActionResult> SaveItinerarySnapshot([FromBody] ItinerarySnapshotDto dto)
        {
            if (dto == null || dto.Items == null)
            {
                return BadRequest("傳入的行程內容為空");
            }

            // 2. 驗證行程 ID 是否合法
            if (dto.ItineraryId <= 0)
            {
                return BadRequest("無效的行程識別碼");
            }

            try
            {
                // 3. 呼叫 Service 執行複雜的版本切換與存檔
                int newVersionId = await _itineraryService.SaveItinerarySnapshotAsync(dto);

                // 4. 回傳成功結果與新的版本 ID
                return Ok(new
                {
                    success = true,
                    message = "行程快照儲存成功，已產生新版本",
                    versionId = newVersionId
                });
            }
            catch (Exception ex)
            {
                var realMessage = ex.InnerException?.Message ?? ex.Message;
                return StatusCode(500, $"儲存快照時發生伺服器錯誤: {realMessage}");
            }
        }
        //POST修改圖片
        [HttpPost("Savephoto/{Id}")]
        public async Task<IActionResult> SavePhoto([FromForm] ItineraryImageDto dto, int Id)
        {
            var imageUrl = await _itineraryService.SaveImagebyid(dto.Image, Id);
            if (imageUrl == null)
            {
                return BadRequest("圖片上傳失敗");
            }
            return Ok(new { url = imageUrl });
        }

        // GET 基於版本找該版本的所有ITEM
        [HttpGet("{VerId}/item")]
        public async Task<IActionResult> GetitembyVer(int VerId)
        {
            if (VerId == 0)
            {
                return BadRequest("沒有該行程");
            }
            var result = await _itineraryService.GetItemByVersionAsync(VerId);
            if (result == null)
            {
                return BadRequest("沒有該行程");
            }
            return Ok(result);
        }

        // DELETE刪除行程，更新行程狀態為「已刪除」
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _itineraryService.SoftDeleteItineraryAsync(id);

            if (!success)
            {
                return NotFound(new { message = $"找不到編號為 {id} 的行程" });
            }

            // 刪除成功，RESTful 慣例回傳 204
            return NoContent();
        }
        //輸出PDF
        [HttpGet("{itineraryId}/export")]
        public async Task<IActionResult> ExportItinerary(int itineraryId)
        {
            var fileBuffer = await _itineraryService.GetExportFileAsync(itineraryId);
            return File(fileBuffer, "application/pdf", $"Itinerary_{itineraryId}.pdf");
        }
        //輸出GOOGLE地圖路線
        [HttpGet("{itineraryId}/day/{dayNumber}")]
        public async Task<ActionResult<DayItineraryDto>> GetDayItinerary(int itineraryId, int dayNumber)
        {
            var result = await _itineraryService.GetDayItineraryAsync(itineraryId, dayNumber);
            if (result == null) return NotFound();
            return Ok(result);
        }
    }
}
