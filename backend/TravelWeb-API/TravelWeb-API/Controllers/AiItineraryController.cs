using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TravelWeb_API.Models.Itinerary.DBContext;
using TravelWeb_API.Models.Itinerary.DTO;
using TravelWeb_API.Models.Itinerary.Service;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace TravelWeb_API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class AiItineraryController : ControllerBase
    {

        private readonly IAIItineraryService _aiItineraryService;
        private readonly TravelContext _context;
        public AiItineraryController(IAIItineraryService aiItineraryService, TravelContext travelContext)
        {
            _aiItineraryService = aiItineraryService;
            _context = travelContext;

        }


        // POST api/<AiItineraryController>
        [HttpPost("generate-ai")]
        public async Task<IActionResult> GenerateWithAi([FromBody] ItineraryCreateDto dto)
        {
            var memberId = User.FindFirst("MemberId")?.Value ?? "tw_user_001";
            if (dto.StartTime == null || dto.EndTime == null)
                return BadRequest(new { message = "請提供開始與結束日期" });
            try
            {
                int totalDays = (dto.EndTime.Value.Date - dto.StartTime.Value.Date).Days + 1;

                List<int> finalPoiIds = new List<int>();

                if (dto.ItemsToPush != null)
                {
                    foreach (var item in dto.ItemsToPush)
                    {
                        if (item.AttractionId.HasValue)
                        {
                            // 已在 DB 的景點直接用
                            finalPoiIds.Add(item.AttractionId.Value);
                        }
                        else if (item.ExternalLocation != null)
                        {
                            // ✅ 第一次：使用者從 Google 選的地點，先建入 DB
                            int attrId = await _aiItineraryService.EnsureAttractionExists(item.ExternalLocation);
                            finalPoiIds.Add(attrId);
                        }
                    }
                }

                if (!finalPoiIds.Any())
                    return BadRequest(new { message = "行程必須包含至少一個景點" });

                var resultId = await _aiItineraryService.GenerateNewItineraryAsync(dto, finalPoiIds, totalDays, memberId);
                return Ok(new { id = resultId, message = "AI 行程生成成功" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "系統發生錯誤", details = ex.Message });
            }
        }


    }
}
