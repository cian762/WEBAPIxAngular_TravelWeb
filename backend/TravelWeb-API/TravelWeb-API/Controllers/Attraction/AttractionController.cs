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
                .Where(a => !a.IsDeleted && a.ApprovalStatus == 1 && a.RegionId != 1000)
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
                .Where(a => a.AttractionId == id && !a.IsDeleted && a.ApprovalStatus == 1 && a.RegionId != 1000)
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

            // ✅ 聚合該景點所有上架票券的圖片（供活動介紹區塊使用）
            var productImages = await _dbContext.AttractionProducts
                .Where(p => p.AttractionId == id && !p.IsDeleted && p.Status == "ACTIVE")
                .SelectMany(p => p.AttractionProductImages)
                .OrderBy(img => img.SortOrder)
                .Select(img => new
                {
                    img.ImageId,
                    img.ImagePath,
                    img.Caption
                })
                .ToListAsync();

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
                attraction.Description,
                attraction.ActivityIntro,
                attraction.Latitude,
                attraction.Longitude,
                attraction.ViewCount,
                LikeCount = likeCount,
                attraction.RegionId,
                attraction.Region.RegionName,
                Images = attraction.Images.Select(i => i.ImagePath).ToList(),
                ProductImages = productImages,
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
        public async Task<IActionResult> LikeAttraction(int id)
        {
            var attractionExists = await _dbContext.Attractions
                .AnyAsync(a => a.AttractionId == id && !a.IsDeleted && a.ApprovalStatus == 1);

            if (!attractionExists)
                return NotFound(new { message = "找不到此景點" });

            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

            var existing = await _dbContext.AttractionLikes
                .FirstOrDefaultAsync(l => l.AttractionId == id && l.IpAddress == ip);

            if (existing != null)
            {
                // 已按過 → 取消按讚（刪除記錄）
                _dbContext.AttractionLikes.Remove(existing);
                await _dbContext.SaveChangesAsync();

                var countAfterUnlike = await _dbContext.AttractionLikes
                    .CountAsync(l => l.AttractionId == id);

                return Ok(new { message = "已取消按讚", likeCount = countAfterUnlike, liked = false });
            }
            else
            {
                // 第一次按 → 新增按讚
                _dbContext.AttractionLikes.Add(new AttractionLike
                {
                    AttractionId = id,
                    IpAddress = ip,
                    CreatedAt = DateTime.Now
                });
                await _dbContext.SaveChangesAsync();

                var countAfterLike = await _dbContext.AttractionLikes
                    .CountAsync(l => l.AttractionId == id);

                return Ok(new { message = "按讚成功", likeCount = countAfterLike, liked = true });
            }
        }

        // ────────────────────────────────────────────
        // GET /api/attraction/{id}/nearby?radius=5&top=8
        // 取得附近景點（依距離排序）
        // ────────────────────────────────────────────
        [HttpGet("{id}/nearby")]
        public async Task<IActionResult> GetNearbyAttractions(
            int id,
            [FromQuery] double radius = 10,
            [FromQuery] int top = 8)
        {
            var current = await _dbContext.Attractions
                .Where(a => a.AttractionId == id && !a.IsDeleted)
                .Select(a => new { a.Latitude, a.Longitude })
                .FirstOrDefaultAsync();

            if (current == null || current.Latitude == null || current.Longitude == null)
                return NotFound(new { message = "找不到此景點或座標未設定" });

            double lat = (double)current.Latitude;
            double lng = (double)current.Longitude;

            // Haversine 公式常數
            const double R = 6371; // 地球半徑 km

            var allAttractions = await _dbContext.Attractions
                .Where(a => a.AttractionId != id
                         && !a.IsDeleted
                         && a.ApprovalStatus == 1
                         && a.RegionId != 1000
                         && a.Latitude != null
                         && a.Longitude != null)
                .Include(a => a.Images)
                .ToListAsync();

            var nearby = allAttractions
                .Select(a =>
                {
                    double aLat = (double)a.Latitude!;
                    double aLng = (double)a.Longitude!;
                    double dLat = (aLat - lat) * Math.PI / 180;
                    double dLng = (aLng - lng) * Math.PI / 180;
                    double hav = Math.Sin(dLat / 2) * Math.Sin(dLat / 2)
                               + Math.Cos(lat * Math.PI / 180)
                               * Math.Cos(aLat * Math.PI / 180)
                               * Math.Sin(dLng / 2) * Math.Sin(dLng / 2);
                    double dist = R * 2 * Math.Atan2(Math.Sqrt(hav), Math.Sqrt(1 - hav));
                    return new { Attraction = a, Distance = dist };
                })
                .Where(x => x.Distance <= radius)
                .OrderBy(x => x.Distance)
                .Take(top)
                .Select(x => new
                {
                    x.Attraction.AttractionId,
                    x.Attraction.Name,
                    x.Attraction.Address,
                    MainImage = x.Attraction.Images.Select(i => i.ImagePath).FirstOrDefault(),
                    DistanceKm = Math.Round(x.Distance, 2)
                })
                .ToList();

            return Ok(nearby);
        }
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
                .Where(a => matchedIds.Contains(a.AttractionId) && !a.IsDeleted && a.ApprovalStatus == 1 && a.RegionId != 1000)
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

        // ────────────────────────────────────────────
        // GET /api/attraction/{id}/related-tickets?top=10
        // 票券推薦：依標籤重疊數 → viewCount → 隨機
        // ────────────────────────────────────────────
        [HttpGet("{id}/related-tickets")]
        public async Task<IActionResult> GetRelatedTickets(int id, [FromQuery] int top = 10)
        {
            // 1. 取得當前景點的標籤
            var currentTags = await _dbContext.AttractionTypeMappings
                .Where(m => m.AttractionId == id)
                .Select(m => m.AttractionTypeId)
                .ToListAsync();

            if (!currentTags.Any())
                return Ok(new List<object>());

            // 2. 找有相同標籤且有上架票券的其他景點
            var candidateAttractionIds = await _dbContext.AttractionTypeMappings
                .Where(m => currentTags.Contains(m.AttractionTypeId)
                         && m.AttractionId != id)
                .Select(m => m.AttractionId)
                .Distinct()
                .ToListAsync();

            if (!candidateAttractionIds.Any())
                return Ok(new List<object>());

            // 3. 撈候選景點（有上架票券）+ 圖片 + 票券
            var candidates = await _dbContext.Attractions
                .Where(a => candidateAttractionIds.Contains(a.AttractionId)
                         && !a.IsDeleted
                         && a.ApprovalStatus == 1
                         && a.RegionId != 1000
                         && a.AttractionProducts.Any(p => !p.IsDeleted && p.Status == "ACTIVE"))
                .Include(a => a.Images)
                .Include(a => a.AttractionProducts.Where(p => !p.IsDeleted && p.Status == "ACTIVE"))
                    .ThenInclude(p => p.TicketTypeCodeNavigation)
                .ToListAsync();

            if (!candidates.Any())
                return Ok(new List<object>());

            // 4. 取每個景點的標籤，計算重疊數（先撈回記憶體再分組）
            var candidateIds = candidates.Select(a => a.AttractionId).ToList();
            var tagData = await _dbContext.AttractionTypeMappings
                .Where(m => candidateIds.Contains(m.AttractionId))
                .Select(m => new { m.AttractionId, m.AttractionTypeId })
                .ToListAsync();

            var tagMap = tagData
                .GroupBy(m => m.AttractionId)
                .ToDictionary(g => g.Key, g => g.Select(m => m.AttractionTypeId).ToList());

            var rng = new Random();

            var result = candidates
                .Select(a =>
                {
                    var tags = tagMap.ContainsKey(a.AttractionId) ? tagMap[a.AttractionId] : new List<int>();
                    int overlap = tags.Count(t => currentTags.Contains(t));
                    var cheapestProduct = a.AttractionProducts
                        .OrderBy(p => p.Price)
                        .FirstOrDefault();
                    return new
                    {
                        a.AttractionId,
                        a.Name,
                        a.ViewCount,
                        MainImage = a.Images.Select(i => i.ImagePath).FirstOrDefault(),
                        TicketTitle = cheapestProduct?.Title,
                        TicketPrice = cheapestProduct?.Price,
                        OriginalPrice = cheapestProduct?.OriginalPrice,
                        TicketTypeName = cheapestProduct?.TicketTypeCodeNavigation?.TicketTypeName,
                        OverlapCount = overlap,
                        SortRandom = rng.Next()
                    };
                })
                .OrderByDescending(x => x.OverlapCount)
                .ThenByDescending(x => x.ViewCount)
                .ThenBy(x => x.SortRandom)
                .Take(top)
                .ToList();

            return Ok(result);
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