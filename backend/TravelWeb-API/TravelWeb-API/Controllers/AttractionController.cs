using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TravelWeb_API.Models.attraction;

namespace TravelWeb_API.Controllers
{
    
    [Route("api/[controller]")]
    [ApiController]
    public class AttractionController : ControllerBase
    {
        private readonly AttractionsContext _dbContext;

        public AttractionController(AttractionsContext dbContext)
        {
            _dbContext = dbContext;
        }

        // 1. 取得所有景點 (用於列表頁)
        [HttpGet]
        public async Task<IActionResult> GetAllAttractions()
        {
            var list = await _dbContext.Attractions.ToListAsync();
            return Ok(list);
        }

        // 2. 根據 ID 取得特定景點 (用於詳細介紹頁)
        [HttpGet("{id}")]
        public async Task<IActionResult> GetAttractionById(int id)
        {
            var attraction = await _dbContext.Attractions.FindAsync(id);
            if (attraction == null) return NotFound("找不到該景點");

            return Ok(attraction);
        }




    }
}
