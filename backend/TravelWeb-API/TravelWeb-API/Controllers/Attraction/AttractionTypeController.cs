using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TravelWeb_API.Models.attraction;



namespace TravelWeb_API.Controllers.Attraction
{
    [ApiController]
    [Route("api/[controller]")]
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



    }
}
