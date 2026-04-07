using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TravelWeb_API.Models.TripProduct;

// [YJ] 景點評論 Controller
namespace TravelWeb_API.Controllers.Attraction
{
    [ApiController]
    [Route("api/[controller]")]
    public class AttractionReviewController : ControllerBase
    {
        private readonly TripDbContext _tripDb;

        public AttractionReviewController(TripDbContext tripDb)
        {
            _tripDb = tripDb;
        }

        // ────────────────────────────────────────────────────────────
        // GET /api/AttractionReview/{attractionId}
        // 取得景點評論列表（含平均分、總數）
        // ────────────────────────────────────────────────────────────
        [HttpGet("{attractionId}")]
        public async Task<IActionResult> GetReviews(int attractionId)
        {
            var reviews = await _tripDb.Database
                .SqlQuery<ReviewDto>($@"
                    SELECT
                        review_id              AS ReviewId,
                        member_id              AS MemberId,
                        member_name            AS MemberName,
                        avatar_url             AS AvatarUrl,
                        CAST(rating AS INT)    AS Rating,
                        title                  AS Title,
                        comment                AS Comment,
                        created_at             AS CreatedAt
                    FROM [Attractions].[AttractionProductReviews]
                    WHERE attraction_id = {attractionId}
                      AND is_deleted = 0
                    ORDER BY created_at DESC
                ")
                .ToListAsync();

            // 計算平均分
            double avgRating = reviews.Count > 0
                ? Math.Round(reviews.Average(r => r.Rating), 1)
                : 0;

            return Ok(new
            {
                attractionId,
                averageRating = avgRating,
                totalCount = reviews.Count,
                reviews
            });
        }

        // ────────────────────────────────────────────────────────────
        // GET /api/AttractionReview/{attractionId}/can-review
        // 確認目前登入會員是否可以評論（已購買 + 尚未評論過）
        // ────────────────────────────────────────────────────────────
        [HttpGet("{attractionId}/can-review")]
        [Authorize]
        public async Task<IActionResult> CanReview(int attractionId)
        {
            var memberId = User.FindFirst("MemberId")?.Value;
            if (string.IsNullOrEmpty(memberId))
                return Unauthorized(new { canReview = false, reason = "請先登入" });

            // 確認是否已評論過
            var alreadyReviewed = await _tripDb.Database
                .SqlQuery<int>($@"
                    SELECT COUNT(1) AS Value
                    FROM [Attractions].[AttractionProductReviews]
                    WHERE attraction_id = {attractionId}
                      AND member_id = {memberId}
                      AND is_deleted = 0
                ")
                .FirstOrDefaultAsync() > 0;

            if (alreadyReviewed)
                return Ok(new { canReview = false, reason = "already_reviewed" });

            // 確認是否有已完成的訂單（購買過此景點的任何票券）
            var hasPurchased = await _tripDb.Orders
                .Where(o => o.MemberId == memberId && o.OrderStatus == "已處理")
                .SelectMany(o => o.OrderItems)
                .AnyAsync(oi => _tripDb.AttractionProducts
                    .Any(p => p.ProductCode == oi.ProductCode
                           && p.AttractionId == attractionId));

            if (!hasPurchased)
                return Ok(new { canReview = false, reason = "not_purchased" });

            return Ok(new { canReview = true, reason = "" });
        }

        // ────────────────────────────────────────────────────────────
        // POST /api/AttractionReview
        // 新增評論（需登入 + 需購買過）
        // ────────────────────────────────────────────────────────────
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddReview([FromBody] AddReviewDto dto)
        {
            var memberId = User.FindFirst("MemberId")?.Value;
            if (string.IsNullOrEmpty(memberId))
                return Unauthorized(new { message = "請先登入" });

            // 驗證評分範圍
            if (dto.Rating < 1 || dto.Rating > 5)
                return BadRequest(new { message = "評分需在 1 ~ 5 之間" });

            // 確認沒評論過
            var alreadyReviewed = await _tripDb.Database
                .SqlQuery<int>($@"
                    SELECT COUNT(1) AS Value
                    FROM [Attractions].[AttractionProductReviews]
                    WHERE attraction_id = {dto.AttractionId}
                      AND member_id = {memberId}
                      AND is_deleted = 0
                ")
                .FirstOrDefaultAsync() > 0;

            if (alreadyReviewed)
                return BadRequest(new { message = "您已評論過此景點" });

            // 確認有購買紀錄
            var hasPurchased = await _tripDb.Orders
                .Where(o => o.MemberId == memberId && o.OrderStatus == "已處理")
                .SelectMany(o => o.OrderItems)
                .AnyAsync(oi => _tripDb.AttractionProducts
                    .Any(p => p.ProductCode == oi.ProductCode
                           && p.AttractionId == dto.AttractionId));

            if (!hasPurchased)
                return Forbid();

            // 取得會員名稱、頭像（快照）
            var memberInfo = await _tripDb.MemberInformations
                .Where(m => m.MemberId == memberId)
                .Select(m => new { m.Name, m.AvatarUrl })
                .FirstOrDefaultAsync();

            string memberName = memberInfo?.Name ?? memberId;
            string? avatarUrl = memberInfo?.AvatarUrl;

            // 寫入評論
            await _tripDb.Database.ExecuteSqlAsync($@"
                INSERT INTO [Attractions].[AttractionProductReviews]
                  (attraction_id, member_id, member_name, avatar_url, rating, title, comment, created_at, is_deleted)
                VALUES
                  ({dto.AttractionId}, {memberId}, {memberName}, {avatarUrl},
                   {dto.Rating}, {dto.Title}, {dto.Comment}, GETDATE(), 0)
            ");

            return Ok(new { message = "評論成功" });
        }
    }

    // ── DTOs ─────────────────────────────────────────────────────────

    /// <summary>評論顯示用 DTO</summary>
    public class ReviewDto
    {
        public int ReviewId { get; set; }
        public string MemberId { get; set; } = "";
        public string? MemberName { get; set; }
        public string? AvatarUrl { get; set; }
        public int Rating { get; set; }
        public string? Title { get; set; }
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>新增評論請求 DTO</summary>
    public class AddReviewDto
    {
        public int AttractionId { get; set; }
        public int Rating { get; set; }          // 1~5
        public string? Title { get; set; }
        public string? Comment { get; set; }
    }
}