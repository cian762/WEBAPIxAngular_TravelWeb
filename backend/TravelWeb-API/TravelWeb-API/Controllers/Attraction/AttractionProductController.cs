using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TravelWeb_API.Models.attraction;

namespace TravelWeb_API.Controllers.Attraction
{
    [ApiController]
    [Route("api/[controller]")]
    public class AttractionProductController : ControllerBase
    {
        private readonly AttractionsContext _dbContext;

        public AttractionProductController(AttractionsContext dbContext)
        {
            _dbContext = dbContext;
        }

        // ────────────────────────────────────────────
        // GET /api/attractionproduct/byattraction/{attractionId}
        // 取得某景點的所有上架售票商品
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
                .OrderBy(p => p.TicketTypeCodeNavigation != null
                    ? p.TicketTypeCodeNavigation.SortOrder
                    : 999)
                .Select(p => new
                {
                    p.ProductId,
                    p.ProductCode,
                    p.Title,
                    p.Price,
                    p.MaxPurchaseQuantity,
                    p.Status,
                    TicketTypeCode = p.TicketTypeCode,
                    TicketTypeName = p.TicketTypeCodeNavigation != null
                        ? p.TicketTypeCodeNavigation.TicketTypeName
                        : null
                })
                .ToListAsync();

            return Ok(products);
        }

        // ────────────────────────────────────────────
        // GET /api/attractionproduct/{productId}
        // 取得單一商品詳細（含票種說明）
        // ────────────────────────────────────────────
        [HttpGet("{productId}")]
        public async Task<IActionResult> GetProductDetail(int productId)
        {
            var product = await _dbContext.AttractionProducts
                .Where(p => p.ProductId == productId && !p.IsDeleted && p.Status == "ACTIVE")
                .Include(p => p.TicketTypeCodeNavigation)
                .Include(p => p.Attraction)
                .FirstOrDefaultAsync();

            if (product == null)
                return NotFound(new { message = "找不到此商品" });

            // 取得商品詳細說明
            var detail = await _dbContext.AttractionProductDetails
                .Where(d => d.ProductId == productId)
                .OrderByDescending(d => d.LastUpdatedAt)
                .Select(d => new
                {
                    d.ContentDetails,
                    d.Notes,
                    d.UsageInstructions
                })
                .FirstOrDefaultAsync();

            // 取得庫存狀態
            var inventory = await _dbContext.ProductInventoryStatuses
                .Where(i => i.ProductId == productId)
                .Select(i => new
                {
                    i.InventoryMode,
                    i.DailyLimit,
                    i.SoldQuantity
                })
                .FirstOrDefaultAsync();

            var result = new
            {
                product.ProductId,
                product.ProductCode,
                product.Title,
                product.Price,
                product.MaxPurchaseQuantity,
                product.Status,
                TicketTypeName = product.TicketTypeCodeNavigation?.TicketTypeName,
                AttractionName = product.Attraction?.Name,
                Detail = detail,
                Inventory = inventory
            };

            return Ok(result);
        }

        // ────────────────────────────────────────────
        // GET /api/attractionproduct/tickettypes
        // 取得所有票種清單（前台下拉選單用）
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
    }
}