using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Security.AccessControl;
using TravelWeb_API.DTO.ActivityDTO;
using TravelWeb_API.Models.ActivityModel;
using TravelWeb_API.QueryParameters.ActivityQueryParameters;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace TravelWeb_API.Services
{
    public class ActivityCardService
    {
        private readonly ActivityDbContext _dbContext;

        public ActivityCardService(ActivityDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<PagedResponseDTO<ActivityCardReponseDTO>> GetSpecificCards(ActivityInfoParameters q) 
       {
            var query = _dbContext.Activities
                .Include(a=>a.Reviews)
                .Where(a => a.SoftDelete == false)
                .AsNoTracking();

            if (q.Keyword != null)
            {
                query = query.Where(a => a.Title == q.Keyword);
            }

            if (q.Type != null && q.Type.Length > 0)
            {
                query = query.Where(a => a.Types.Any(t => q.Type!.Contains(t.ActivityType)));
            }

            if (q.Region != null && q.Region.Length > 0)
            {
                query = query.Where(a => a.Regions.Any(r => q.Region!.Contains(r.RegionName)));
                   
            }

            if (q.Start != null && q.End != null)
            {
                query = query.Where(a =>a.StartTime <= q.End && a.EndTime >= q.Start);
            }


            DateOnly today = DateOnly.FromDateTime(DateTime.Today);

            if (q.OrderByParam == "hot")
            {
                query = query.OrderByDescending(a => a.Reviews.Any()? a.Reviews.Select(r=>r.ReviewId).Count() : 0);
            }
            else if (q.OrderByParam == "rating")
            {
                query = query.OrderByDescending(a => a.Reviews.Any() ? a.Reviews.Average(r => r.Rating) : 0);
            }
            else if (q.OrderByParam == "latest")
            {
                query = query.OrderBy(a => Math.Abs(EF.Functions.DateDiffDay(today, a.StartTime)!.Value));
            }


            var totalRecords = query.Where(a => a.EndTime >= today).Count();

            var ans = await query
                .Where(a => a.EndTime >= today)
                .Skip((q.PageNumber - 1) * q.PageSize)
                .Take(q.PageSize)
                .Select(a => new ActivityCardReponseDTO
                {
                    ActivityId = a.ActivityId,
                    Title = a.Title,
                    Type = a.Types.Select(t => t.ActivityType).ToList(),
                    Region = a.Regions.Select(r => r.RegionName).ToList(),
                    Start = a.StartTime,
                    End = a.EndTime,
                    CoverImageUrl = a.ActivityImages
                                    .Where(i => i.IsCoverImage != false)
                                    .Select(i => i.ImageUrl)
                                    .FirstOrDefault(),
                    ViewCount = a.ViewCount,
                    CommentCount = a.Reviews.Where(r=>r.IsSoftDeleted==false).Count(),
                    AverageRating = (float)Math.Round((a.Reviews.Any() ? a.Reviews.Where(r=>r.IsSoftDeleted==false).Average(r => r.Rating) : 0),1),
                    ReferencePrice = a.ActivityTicketDetails
                    .Where(d => d.ProductCodeNavigation.TicketCategoryId == 2)
                    .Select(d => d.ProductCodeNavigation.CurrentPrice)
                    .FirstOrDefault() ?? 0,
                })
                .ToListAsync();

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
