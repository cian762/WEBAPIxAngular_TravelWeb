using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Security.AccessControl;
using TravelWeb_API.DTO.ActivityDTO;
using TravelWeb_API.Models.ActivityModel;
using TravelWeb_API.Models.TripProduct;
using TravelWeb_API.QueryParameters.ActivityQueryParameters;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace TravelWeb_API.Services
{
    public class ActivityCardService
    {
        private readonly ActivityDbContext _dbContext;
        private readonly TripDbContext _tripDbContext;

        public ActivityCardService(ActivityDbContext dbContext, TripDbContext tripDbContext)
        {
            _dbContext = dbContext;
            _tripDbContext = tripDbContext;
        }

        public async Task<PagedResponseDTO<ActivityCardReponseDTO>> GetSpecificCards(ActivityInfoParameters q)
        {
            var query = _dbContext.Activities
                .Where(a => a.SoftDelete == false)
                .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(q.Keyword))
            {
                query = query.Where(a => a.Title == q.Keyword);
            }

            if (q.Type != null && q.Type.Length > 0)
            {
                query = query.Where(a => a.Types.Any(t => q.Type.Contains(t.ActivityType)));
            }

            if (q.Region != null && q.Region.Length > 0)
            {
                query = query.Where(a => a.Regions.Any(r => q.Region.Contains(r.RegionName)));
            }

            if (q.Start != null && q.End != null)
            {
                query = query.Where(a => a.StartTime <= q.End && a.EndTime >= q.Start);
            }

            DateOnly today = DateOnly.FromDateTime(DateTime.Today);

            if (q.OrderByParam == "hot")
            {
                query = query.OrderByDescending(a => a.Reviews.Count(r => r.IsSoftDeleted == false));
            }
            else if (q.OrderByParam == "rating")
            {
                query = query.OrderByDescending(a =>
                    a.Reviews.Any(r => r.IsSoftDeleted == false)
                        ? a.Reviews.Where(r => r.IsSoftDeleted == false).Average(r => r.Rating)
                        : 0);
            }
            else if (q.OrderByParam == "latest")
            {
                query = query.OrderBy(a => Math.Abs(EF.Functions.DateDiffDay(today, a.StartTime) ?? 0));
            }

            var filteredQuery = query.Where(a => a.EndTime >= today);

            var totalRecords = await filteredQuery.CountAsync();

            // 先查這一頁活動的基本資料 + ProductCodes
            var pageData = await filteredQuery
                .Skip((q.PageNumber - 1) * q.PageSize)
                .Take(q.PageSize)
                .Select(a => new
                {
                    ActivityId = a.ActivityId,
                    Title = a.Title,
                    Type = a.Types.Select(t => t.ActivityType).ToList(),
                    Region = a.Regions.Select(r => r.RegionName).ToList(),
                    Start = a.StartTime,
                    End = a.EndTime,
                    CoverImageUrl = a.ActivityImages
                        .Where(i => i.IsCoverImage == true)
                        .Select(i => i.ImageUrl)
                        .FirstOrDefault(),
                    ViewCount = a.ViewCount,
                    CommentCount = a.Reviews.Count(r => r.IsSoftDeleted == false),
                    AverageRating = a.Reviews.Any(r => r.IsSoftDeleted == false)
                        ? (float)Math.Round(
                            a.Reviews
                                .Where(r => r.IsSoftDeleted == false)
                                .Average(r => r.Rating), 1)
                        : 0,
                    ReferencePrice = a.ActivityTicketDetails
                        .Where(d => d.ProductCodeNavigation.TicketCategoryId == 2)
                        .Select(d => d.ProductCodeNavigation.CurrentPrice)
                        .FirstOrDefault() ?? 0,
                    ProductCodes = a.ActivityTicketDetails
                        .Select(at => at.ProductCode)
                        .Where(code => code != null)
                        .ToList()
                })
                .ToListAsync();

            // 收集這一頁所有活動對應的 ProductCode
            var allProductCodes = pageData
                .SelectMany(x => x.ProductCodes)
                .Where(code => !string.IsNullOrWhiteSpace(code))
                .Distinct()
                .ToList();

            // 查每個 ProductCode 的銷售數量
            var sellCountByProductCode = await _tripDbContext.OrderItems
                .Where(oi => oi.ProductCode != null && allProductCodes.Contains(oi.ProductCode))
                .GroupBy(oi => oi.ProductCode!)
                .Select(g => new
                {
                    ProductCode = g.Key,
                    SellCount = g.Count()
                })
                .ToDictionaryAsync(x => x.ProductCode, x => x.SellCount);

            // 組成最後 DTO
            var ans = pageData
                .Select(x => new ActivityCardReponseDTO
                {
                    ActivityId = x.ActivityId,
                    Title = x.Title,
                    Type = x.Type,
                    Region = x.Region,
                    Start = x.Start,
                    End = x.End,
                    CoverImageUrl = x.CoverImageUrl,
                    ViewCount = x.ViewCount,
                    CommentCount = x.CommentCount,
                    AverageRating = x.AverageRating,
                    ReferencePrice = x.ReferencePrice,
                    SellCount = x.ProductCodes.Sum(code =>
                        code != null && sellCountByProductCode.TryGetValue(code, out var count)
                            ? count
                            : 0)
                })
                .ToList();

            return new PagedResponseDTO<ActivityCardReponseDTO>(ans, q.PageNumber, totalRecords, q.PageSize);
        }

        public async Task<List<string?>?> SearchSpecificCards(string searchText)
        {
            if (searchText.IsNullOrEmpty()) return null;

            var ans = await _dbContext.Activities
                .Where(a => a.SoftDelete == false)
                .Where(a => a.EndTime >= DateOnly.FromDateTime(DateTime.Today))
                .Where(a => a.Title!.StartsWith(searchText))
                .OrderBy(a => a.Title!.Length)
                .Select(a => a.Title)
                .ToListAsync();

            //如果 ans 數量小於 5 個，就再抓取其他 Title 首字不為 searchText，但 Title Body 中包含關鍵字的活動
            if (ans.Count < 5)
            {
                var containsSuggestions = await _dbContext.Activities
                    .Where(p => !p.Title!.StartsWith(searchText) && p.Title.Contains(searchText))
                    .Take(5)
                    .Select(a => a.Title)
                    .ToListAsync();

                ans.AddRange(containsSuggestions);
            }


            return ans;
        }



    }
}
