using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TravelWeb_API.Models.attraction;

namespace TravelWeb_API.Controllers.Attraction
{
    
    [Route("api/[controller]")]
    [ApiController]
    //[ApiExplorerSettings(GroupName = "Attraction")] // ← 加這行
    public class AttractionController : ControllerBase
    {
        private readonly AttractionsContext _dbContext;

        public AttractionController(AttractionsContext dbContext)
        {
            _dbContext = dbContext;
        }

        // ────────────────────────────────────────────
        // 私有輔助：遞迴取得所有子地區 ID
        // ────────────────────────────────────────────
        private async Task<List<int>> GetAllChildRegionIds(int regionId)
        {
            var result = new List<int> { regionId };
            var children = await _dbContext.TagsRegions
                .Where(r => r.Uid == regionId)
                .Select(r => r.RegionId)
                .ToListAsync();

            foreach (var child in children)
                result.AddRange(await GetAllChildRegionIds(child));

            return result;
        }

        // ────────────────────────────────────────────
        // GET /api/attraction/regions
        // 對應畫面：景點首頁的五個大地區 Tab
        // 北部 / 中部 / 南部 / 東部 / 離島
        // ────────────────────────────────────────────
        [HttpGet("regions")]
        public async Task<IActionResult> GetTopRegions()
        {
            var regions = await _dbContext.TagsRegions
                .Where(r => r.Uid == null)
                .OrderBy(r => r.RegionId)
                .Select(r => new
                {
                    r.RegionId,
                    r.RegionName
                })
                .ToListAsync();

            return Ok(regions);
        }

        // ────────────────────────────────────────────
        // GET /api/attraction/regions/{regionId}/cities
        // 對應畫面：點選「北部」後展開台北市/新北市...圖卡
        // ────────────────────────────────────────────
        [HttpGet("regions/{regionId}/cities")]
        public async Task<IActionResult> GetSubRegions(int regionId)
        {
            var subs = await _dbContext.TagsRegions
                .Where(r => r.Uid == regionId)
                .OrderBy(r => r.RegionId)
                .Select(r => new
                {
                    r.RegionId,
                    r.RegionName
                })
                .ToListAsync();

            if (!subs.Any())
                return NotFound(new { message = "此地區沒有下層地區" });

            return Ok(subs);
        }

        // ────────────────────────────────────────────
        // GET /api/attraction
        // 對應畫面：台北市景點卡片列表
        // 支援 QueryString: regionId, typeId, keyword
        // 卡片顯示：名稱、主圖、分類Tag、ViewCount（👁 數字）
        // ────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> GetAttractions(
            [FromQuery] int? regionId,
            [FromQuery] int? typeId,
            [FromQuery] string? keyword)
        {
            var query = _dbContext.Attractions
                .Where(a => !a.IsDeleted && a.ApprovalStatus == 1)
                .AsQueryable();

            // 地區篩選（遞迴取所有子地區）
            if (regionId.HasValue)
            {
                var allRegionIds = await GetAllChildRegionIds(regionId.Value);
                query = query.Where(a => allRegionIds.Contains(a.RegionId));
            }

            // 景點分類 Tab 篩選
            if (typeId.HasValue)
            {
                var matchedIds = _dbContext.AttractionTypeMappings
                    .Where(m => m.AttractionTypeId == typeId.Value)
                    .Select(m => m.AttractionId);
                query = query.Where(a => matchedIds.Contains(a.AttractionId));
            }

            // 關鍵字搜尋
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                //搜景點名稱和地址
                //query = query.Where(a =>
                //    a.Name.Contains(keyword) ||
                //    a.Address != null && a.Address.Contains(keyword));

                //只搜景點名稱
                query = query.Where(a => a.Name.Contains(keyword));
            }

            // Step 1：先撈景點基本資料
            var attractionList = await query
                .Include(a => a.Images)
                .Include(a => a.Region)
                .OrderByDescending(a => a.ViewCount)
                .ToListAsync();

            // Step 2：撈所有相關景點的分類（避免 Select 裡做子查詢）
            var attractionIds = attractionList.Select(a => a.AttractionId).ToList();

            var typeMappings = await _dbContext.AttractionTypeMappings
                .Where(m => attractionIds.Contains(m.AttractionId))
                .Select(m => new
                {
                    m.AttractionId,
                    m.AttractionTypeId,
                    m.AttractionType.AttractionTypeName
                })
                .ToListAsync();

            // Step 3：組合回傳結果
            var attractions = attractionList.Select(a => new
            {
                a.AttractionId,
                a.Name,
                a.Address,
                a.Phone,
                a.Website,
                a.BusinessHours,
                a.ClosedDaysNote,
                a.Latitude,
                a.Longitude,
                a.ViewCount,
                a.RegionId,
                a.Region.RegionName,
                MainImage = a.Images.Select(i => i.ImagePath).FirstOrDefault(),
                Types = typeMappings
                    .Where(m => m.AttractionId == a.AttractionId)
                    .Select(m => new
                    {
                        m.AttractionTypeId,
                        m.AttractionTypeName
                    }).ToList()
            }).ToList();

            return Ok(attractions);
        }

        // ────────────────────────────────────────────
        // GET /api/attraction/{id}
        // 對應畫面：景點詳細頁
        // 同時將 ViewCount + 1（每次進入詳細頁觸發）
        // ────────────────────────────────────────────
        [HttpGet("{id}")]
        public async Task<IActionResult> GetAttractionDetail(int id)
        {
            var attraction = await _dbContext.Attractions
                .Where(a => a.AttractionId == id && !a.IsDeleted && a.ApprovalStatus == 1)
                .Include(a => a.Images)
                .Include(a => a.Region)
                .Include(a => a.AttractionProducts.Where(p => !p.IsDeleted && p.Status == "ACTIVE"))
                    .ThenInclude(p => p.TicketTypeCodeNavigation)
                .FirstOrDefaultAsync();

            if (attraction == null)
                return NotFound(new { message = "找不到此景點" });

            // ✅ 點擊數 +1
            attraction.ViewCount++;
            await _dbContext.SaveChangesAsync();

            var types = await _dbContext.AttractionTypeMappings
                .Where(m => m.AttractionId == id)
                .Select(m => new
                {
                    m.AttractionTypeId,
                    m.AttractionType.AttractionTypeName
                })
                .ToListAsync();

            // ✅ 取得目前按讚數
            var likeCount = await _dbContext.AttractionLikes
                .CountAsync(l => l.AttractionId == id);

            var result = new
            {
                attraction.AttractionId,
                attraction.Name,
                attraction.Address,
                attraction.Phone,
                attraction.Website,
                attraction.BusinessHours,
                attraction.ClosedDaysNote,
                attraction.TransportInfo,
                attraction.Latitude,
                attraction.Longitude,
                attraction.ViewCount,
                LikeCount = likeCount,              // 詳細頁右上角 278 👍
                attraction.RegionId,
                attraction.Region.RegionName,
                Images = attraction.Images.Select(i => i.ImagePath).ToList(),
                Types = types,
                Products = attraction.AttractionProducts.Select(p => new
                {
                    p.ProductId,
                    p.ProductCode,
                    p.Title,
                    p.Price,
                    p.Status,
                    p.MaxPurchaseQuantity,
                    TicketTypeName = p.TicketTypeCodeNavigation != null
                        ? p.TicketTypeCodeNavigation.TicketTypeName
                        : null
                }).ToList()
            };

            return Ok(result);
        }

        // ────────────────────────────────────────────
        // POST /api/attraction/{id}/like
        // 對應畫面：詳細頁右上角按讚按鈕（不需登入）
        // Body: { "ipAddress": "xxx.xxx.xxx.xxx" }
        // 同一 IP 對同一景點只能按一次
        // ────────────────────────────────────────────
        [HttpPost("{id}/like")]
        public async Task<IActionResult> LikeAttraction(int id, [FromBody] LikeRequest request)
        {
            var attractionExists = await _dbContext.Attractions
                .AnyAsync(a => a.AttractionId == id && !a.IsDeleted && a.ApprovalStatus == 1);

            if (!attractionExists)
                return NotFound(new { message = "找不到此景點" });

            // 檢查同 IP 是否已按讚
            var alreadyLiked = await _dbContext.AttractionLikes
                .AnyAsync(l => l.AttractionId == id && l.IpAddress == request.IpAddress);

            var likeCount = await _dbContext.AttractionLikes
                .CountAsync(l => l.AttractionId == id);

            if (alreadyLiked)
                return Ok(new { message = "已經按過讚了", likeCount, liked = true });

            // 新增按讚記錄
            _dbContext.AttractionLikes.Add(new AttractionLike
            {
                AttractionId = id,
                IpAddress = request.IpAddress,
                CreatedAt = DateTime.Now
            });
            await _dbContext.SaveChangesAsync();

            likeCount++;
            return Ok(new { message = "按讚成功", likeCount, liked = true });
        }

        // ────────────────────────────────────────────
        // GET /api/attraction/bytype/{typeId}
        // 對應畫面：點景點分類 Tag 篩選全台該類型景點
        // 例如點「#歷史古蹟」→ 顯示全台古蹟景點
        // ────────────────────────────────────────────
        [HttpGet("bytype/{typeId}")]
        public async Task<IActionResult> GetAttractionsByType(int typeId)
        {
            var typeExists = await _dbContext.AttractionTypeCategories
                .AnyAsync(t => t.AttractionTypeId == typeId);

            if (!typeExists)
                return NotFound(new { message = "找不到此景點類型" });

            var matchedIds = await _dbContext.AttractionTypeMappings
                .Where(m => m.AttractionTypeId == typeId)
                .Select(m => m.AttractionId)
                .ToListAsync();

            var attractionList = await _dbContext.Attractions
                .Where(a => matchedIds.Contains(a.AttractionId) && !a.IsDeleted && a.ApprovalStatus == 1)
                .Include(a => a.Images)
                .Include(a => a.Region)
                .OrderByDescending(a => a.ViewCount)
                .ToListAsync();

            var attractionIds = attractionList.Select(a => a.AttractionId).ToList();

            var typeMappings = await _dbContext.AttractionTypeMappings
                .Where(m => attractionIds.Contains(m.AttractionId))
                .Select(m => new
                {
                    m.AttractionId,
                    m.AttractionTypeId,
                    m.AttractionType.AttractionTypeName
                })
                .ToListAsync();

            var attractions = attractionList.Select(a => new
            {
                a.AttractionId,
                a.Name,
                a.Address,
                a.ViewCount,
                a.Region.RegionName,
                MainImage = a.Images.Select(i => i.ImagePath).FirstOrDefault(),
                Types = typeMappings
                    .Where(m => m.AttractionId == a.AttractionId)
                    .Select(m => new
                    {
                        m.AttractionTypeId,
                        m.AttractionTypeName
                    }).ToList()
            }).ToList();

            return Ok(attractions);
        }
    }
    // ────────────────────────────────────────────
    // DTO：POST /like 的 Body 格式
    // ────────────────────────────────────────────
    public class LikeRequest
    {
        public string? IpAddress { get; set; }
    }
}
    

