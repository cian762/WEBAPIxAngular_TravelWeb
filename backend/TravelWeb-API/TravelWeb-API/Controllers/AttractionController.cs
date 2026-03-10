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

        // 1. 取得所有景點（分頁 + 篩選）
        [HttpGet]
        public async Task<IActionResult> GetAttractions(
            [FromQuery] int? regionId,
            [FromQuery] string? areaId,
            [FromQuery] string? keyword,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 12)
        {
            var query = _dbContext.Attractions
                .Where(a => !a.IsDeleted && a.ApprovalStatus == 1)
                .Include(a => a.Images)
                .Include(a => a.Region)
                .AsQueryable();

            if (regionId.HasValue)
                query = query.Where(a => a.RegionId == regionId);

            if (!string.IsNullOrEmpty(areaId))
                query = query.Where(a => a.AreaId == areaId);

            if (!string.IsNullOrEmpty(keyword))
                query = query.Where(a =>
                    a.Name.Contains(keyword) ||
                    (a.Address != null && a.Address.Contains(keyword)));

            var total = await query.CountAsync();

            var list = await query
                .OrderBy(a => a.AttractionId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(a => new
                {
                    a.AttractionId,
                    a.Name,
                    a.Address,
                    a.RegionId,
                    a.AreaId,
                    a.Latitude,
                    a.Longitude,
                    a.Phone,
                    a.Website,
                    a.BusinessHours,
                    RegionName = a.Region.RegionName,
                    MainImage = a.Images
                        .Select(i => i.ImagePath)
                        .FirstOrDefault()
                })
                .ToListAsync();

            return Ok(new { total, page, pageSize, data = list });
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
