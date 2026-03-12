using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
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

        public PagedResponseDTO<ActivityCardReponseDTO> GetCards(PagedQueryParameters q) 
        {
            var totalRecords = _dbContext.Activities.Count(a => a.SoftDelete == false);

            var ans = _dbContext.Activities
                .Where(a => a.SoftDelete == false)
                .Skip((q.PageNumber - 1) * q.PageSize)
                .Take(q.PageSize)
                .Select(a => new ActivityCardReponseDTO
                {
                    ActivityId = a.ActivityId,
                    Title = a.Title,
                    Type = a.Types.Select(t => t.ActivityType),
                    Region = a.Regions.Select(r => r.RegionName),
                    Start = a.StartTime,
                    End = a.EndTime,
                })
                .ToList();

            return new PagedResponseDTO<ActivityCardReponseDTO>(ans,q.PageNumber,q.PageSize,totalRecords);
        }

        public PagedResponseDTO<ActivityCardReponseDTO> GetSpecificCards(ActivityInfoParameters q) 
        {
            var query = _dbContext.Activities
                .Where(a => a.SoftDelete == false);

            var totalRecords = query.Count();

            if (!q.Type.IsNullOrEmpty())
            {
                query = query.Where(a => a.Types.Any(t => t.ActivityType == q.Type));
            }

            if (!q.Region.IsNullOrEmpty())
            {
                query = query.Where(a => a.Regions.Any(r => r.RegionName == q.Region));
            }

            if (q.Start != null && q.End != null)
            {
                query = query.Where(a => a.StartTime >= q.Start && a.EndTime <= q.End);
            }

            DateOnly today = DateOnly.FromDateTime(DateTime.Today);
            
            //if(OrderByPopularity) result = result.OrderByDescending(a => a.Popularity);

            if (q.IsLatest)
            {
                query = query.OrderBy(a => EF.Functions.DateDiffDay(today, a.StartTime));
            }
            else if (q.IsObsolete)
            {
                query = query.OrderBy(a => a.StartTime);
            }
            else
            {
                query = query.OrderBy(a => a.ActivityId);
            }


            var ans = query
                .Skip((q.PageNumber - 1) * q.PageSize)
                .Take(q.PageSize)
                .Select(a => new ActivityCardReponseDTO
                {
                    ActivityId = a.ActivityId,
                    Title = a.Title,
                    Type = a.Types.Select(t => t.ActivityType),
                    Region = a.Regions.Select(r => r.RegionName),
                    Start = a.StartTime,
                    End = a.EndTime,
                }).ToList();

            return new PagedResponseDTO<ActivityCardReponseDTO>(ans, q.PageNumber, q.PageSize, totalRecords);

        }

        public PagedResponseDTO<ActivityCardReponseDTO>? SearchSpecificCards(ActivityInfoParameters query)
        {
            if (query.Keyword.IsNullOrEmpty()) return null;
            
            //撈取符合 keyword 和狀態為非軟刪除的 Activity
            var q = _dbContext.Activities
                .Include(a=>a.Types)
                .Include(a=>a.Regions)
                .Where(a => a.Title!.Contains(query.Keyword!) && a.SoftDelete == false)
                .AsEnumerable();

            //拿到所有符合 keyword 的活動數量
            var totalRecords = q.Count();

            var ans = q
            .OrderBy(a=>a.ActivityId)
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(a => new ActivityCardReponseDTO
            {
                ActivityId = a.ActivityId,
                Title = a.Title,
                Type = a.Types.Select(t => t.ActivityType),
                Region = a.Regions.Select(r => r.RegionName),
                Start = a.StartTime,
                End = a.EndTime,
            }).ToList();

            return new PagedResponseDTO<ActivityCardReponseDTO>(ans, query.PageNumber, query.PageSize, totalRecords);
        }



    }
}
