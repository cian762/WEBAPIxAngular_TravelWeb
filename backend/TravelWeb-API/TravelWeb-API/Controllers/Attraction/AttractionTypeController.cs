using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TravelWeb_API.Models.attraction;



namespace TravelWeb_API.Controllers.Attraction
{
    [ApiController]
    [Route("api/[controller]")]
    //[ApiExplorerSettings(GroupName = "Attraction")] // ← 加這行
    public class AttractionTypeController : ControllerBase
    {
        private readonly AttractionsContext _dbContext;

        public AttractionTypeController(AttractionsContext dbContext)
        {
            _dbContext = dbContext;
        }

        // ────────────────────────────────────────────
        // GET /api/attractiontype
        // 取得所有景點分類（前台 Tab 用）
        // ────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> GetAllTypes()
        {
            var types = await _dbContext.AttractionTypeCategories
                .OrderBy(t => t.AttractionTypeId)
                .Select(t => new
                {
                    t.AttractionTypeId,
                    t.AttractionTypeName
                })
                .ToListAsync();

            return Ok(types);
        }


        // ────────────────────────────────────────────
        // GET /api/attractiontype/{id}
        // 取得單一分類（可選，備用）
        // ────────────────────────────────────────────
        [HttpGet("{id}")]
        public async Task<IActionResult> GetTypeById(int id)
        {
            var type = await _dbContext.AttractionTypeCategories
                .Where(t => t.AttractionTypeId == id)
                .Select(t => new
                {
                    t.AttractionTypeId,
                    t.AttractionTypeName
                })
                .FirstOrDefaultAsync();

            if (type == null)
                return NotFound(new { message = "找不到此景點類型" });

            return Ok(type);
        }
    }
}
