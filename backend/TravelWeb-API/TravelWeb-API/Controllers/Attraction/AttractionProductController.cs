using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TravelWeb_API.Models.attraction;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
//沒有更新
namespace TravelWeb_API.Controllers.Attraction
{
    [ApiController]
    [Route("api/[controller]")]
    //[ApiExplorerSettings(GroupName = "Attraction")] // ← 加這行
    public class AttractionProductController : ControllerBase
    {
        private readonly AttractionsContext _dbContext;

        public AttractionProductController(AttractionsContext dbContext)
        {
            _dbContext = dbContext;
        }

        // ────────────────────────────────────────────
        // GET /api/attractionproduct/byattraction/{attractionId}
        // 取得某景點的所有上架售票商品（含 Tags、原價、有效天數）
        // ────────────────────────────────────────────
        [HttpGet("byattraction/{attractionId}")]
        public async Task<IActionResult> GetProductsByAttraction(int attractionId)
        {
            var attractionExists = await _dbContext.Attractions
                .AnyAsync(a => a.AttractionId == attractionId && !a.IsDeleted && a.ApprovalStatus == 1);

            if (!attractionExists)
                return NotFound(new { message = "找不到此景點" });

            var products = await _dbContext.AttractionProducts
                .Where(p => p.AttractionId == attractionId && !p.IsDeleted && p.Status == "ACTIVE")
                .Include(p => p.TicketTypeCodeNavigation)
                .Include(p => p.Tags)
                .OrderBy(p => p.TicketTypeCodeNavigation != null
                    ? p.TicketTypeCodeNavigation.SortOrder
                    : 999)
                .Select(p => new
                {
                    p.ProductId,
                    p.ProductCode,
                    p.Title,
                    p.Price,
                    p.OriginalPrice,
                    p.ValidityDays,
                    p.MaxPurchaseQuantity,
                    p.Status,
                    TicketTypeCode = p.TicketTypeCode,
                    TicketTypeName = p.TicketTypeCodeNavigation != null
                        ? p.TicketTypeCodeNavigation.TicketTypeName
                        : null,
                    Tags = p.Tags.Select(t => new { t.TagName, t.Description }).ToList()
                })
                .ToListAsync();

            return Ok(products);
        }

        // ────────────────────────────────────────────
        // GET /api/attractionproduct/{productId}
        // 取得單一商品詳細（含新增欄位、圖片）
        // 用於方案詳情側邊面板
        // ────────────────────────────────────────────
        [HttpGet("{productId}")]
        public async Task<IActionResult> GetProductDetail(int productId)
        {
            var product = await _dbContext.AttractionProducts
                .Where(p => p.ProductId == productId && !p.IsDeleted && p.Status == "ACTIVE")
                .Include(p => p.TicketTypeCodeNavigation)
                .Include(p => p.Attraction)
                .Include(p => p.Tags)
                .FirstOrDefaultAsync();

            if (product == null)
                return NotFound(new { message = "找不到此商品" });


            // 詳細說明
            // detail 的 Select 補上 ValidityNote
            var detail = await _dbContext.AttractionProductDetails
                .Where(d => d.ProductId == productId)
                .OrderByDescending(d => d.LastUpdatedAt)
                .Select(d => new
                {
                    d.ContentDetails,
                    d.Notes,
                    d.UsageInstructions,
                    d.Includes,
                    d.Excludes,
                    d.Eligibility,
                    d.CancelPolicy,
                    d.ValidityNote    // ← 補上這行
                })
                .FirstOrDefaultAsync();

            // 票券圖片
            var images = await _dbContext.AttractionProductImages
                .Where(i => i.ProductId == productId)
                .OrderBy(i => i.SortOrder)
                .Select(i => new
                {
                    i.ImageId,
                    i.ImagePath,
                    i.Caption
                })
                .ToListAsync();

            var result = new
            {
                product.ProductId,
                product.ProductCode,
                product.Title,
                product.Price,
                product.OriginalPrice,
                product.ValidityDays,
                product.MaxPurchaseQuantity,
                TicketTypeName = product.TicketTypeCodeNavigation?.TicketTypeName,
                AttractionName = product.Attraction?.Name,
                // Tags 改成物件
                Tags = product.Tags.Select(t => new { t.TagName, t.Description }).ToList(),
                Detail = detail,
                Images = images
            };

            return Ok(result);
        }

        // ────────────────────────────────────────────
        // GET /api/attractionproduct/stock/{productCode}
        // 取得庫存（以 StockInRecords.remaining_stock 加總）
        // ────────────────────────────────────────────
        [HttpGet("stock/{productCode}")]
        public async Task<IActionResult> GetStock(string productCode)
        {
            var stock = await _dbContext.StockInRecords
                .Where(s => s.ProductCode == productCode)
                .SumAsync(s => (int?)s.RemainingStock) ?? 0;

            return Ok(new { productCode, remainingStock = stock });
        }

        // ────────────────────────────────────────────
        // GET /api/attractionproduct/tickettypes
        // 取得所有票種清單
        // ────────────────────────────────────────────
        [HttpGet("tickettypes")]
        public async Task<IActionResult> GetTicketTypes()
        {
            var types = await _dbContext.TicketTypes
                .OrderBy(t => t.SortOrder)
                .Select(t => new
                {
                    t.TicketTypeCode,
                    t.TicketTypeName,
                    t.SortOrder
                })
                .ToListAsync();

            return Ok(types);
        }
  
        

        // ────────────────────────────────────────────
        // POST /api/attractionproduct/{productId}/favorite
        // 切換票券收藏（需登入）
        // ────────────────────────────────────────────
        [Authorize]
        [HttpPost("{productId}/favorite")]
        public async Task<IActionResult> ToggleFavorite(int productId)
        {
            var memberId = User.FindFirst("MemberId")?.Value
                           ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(memberId))
                return Unauthorized(new { message = "請先登入" });

            var productExists = await _dbContext.AttractionProducts
                .AnyAsync(p => p.ProductId == productId && !p.IsDeleted);
            if (!productExists)
                return NotFound(new { message = "找不到此票券" });

            var existing = await _dbContext.AttractionProductFavorites
                .FirstOrDefaultAsync(f => f.ProductId == productId && f.UserId == memberId);

            if (existing != null)
            {
                _dbContext.AttractionProductFavorites.Remove(existing);
                await _dbContext.SaveChangesAsync();
                return Ok(new { isFavorited = false, message = "已取消收藏" });
            }

            _dbContext.AttractionProductFavorites.Add(new AttractionProductFavorite
            {
                ProductId = productId,
                UserId = memberId,
                CreatedAt = DateTime.Now
            });
            await _dbContext.SaveChangesAsync();
            return Ok(new { isFavorited = true, message = "已加入收藏" });
        }

        // ────────────────────────────────────────────
        // GET /api/attractionproduct/my-favorites
        // 取得當前登入會員收藏的票券 ID 清單
        // ────────────────────────────────────────────
        [Authorize]
        [HttpGet("my-favorites")]
        public async Task<IActionResult> GetMyFavorites()
        {
            var memberId = User.FindFirst("MemberId")?.Value
                           ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(memberId))
                return Unauthorized();

            var ids = await _dbContext.AttractionProductFavorites
                .Where(f => f.UserId == memberId)
                .Select(f => f.ProductId)
                .ToListAsync();

            return Ok(ids);
        }


    }
}