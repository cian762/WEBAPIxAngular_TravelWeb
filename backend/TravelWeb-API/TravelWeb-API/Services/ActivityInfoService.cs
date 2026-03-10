using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using TravelWeb_API.DTO.ActivityDTO;
using TravelWeb_API.Models.ActivityModel;
using TravelWeb_API.QueryParameters.ActivityQueryParameters;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace TravelWeb_API.Services
{
    public class ActivityInfoService
    {
        private readonly ActivityDbContext _dbContext;

        public ActivityInfoService(ActivityDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public PagedResponseDTO<ActivityInfoReponseDTO> GetActivities(ActivityInfoParameters q) 
        {
            var totalRecords = _dbContext.Activities.Count(a => a.SoftDelete == false);

            var query = _dbContext.Activities
                .Where(a => a.SoftDelete == false);


            //if(OrderByPopularity) result = result.OrderByDescending(a => a.Popularity);
            
            if (q.IsLatest)
            {
                query = query.OrderByDescending(a => a.StartTime);
            }
            else if (q.IsObsolete)
            {
                query = query.OrderBy(a => a.StartTime);
            }
            else 
            {
                query = query.OrderBy(a => a.ActivityId);
            }


            var result = query
            .Skip((q.PageNumber - 1) * q.PageSize)
            .Take(q.PageSize)
            .Select(a => new ActivityInfoReponseDTO
            {
                ActivityId = a.ActivityId,
                Title = a.Title,
                Type = a.Types.Select(t => t.ActivityType),
                Region = a.Regions.Select(r => r.RegionName),
                Start = a.StartTime,
                End = a.EndTime,
            })
            .ToList();

            return new PagedResponseDTO<ActivityInfoReponseDTO>(result,q.PageNumber,q.PageSize,totalRecords);

        }

        public PagedResponseDTO<ActivityInfoReponseDTO> GetSpecificActivities(ActivityInfoParameters q) 
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
            

            if (q.IsLatest)
            {
                query = query.OrderByDescending(a => a.StartTime);
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
                .Select(a => new ActivityInfoReponseDTO
                {
                    ActivityId = a.ActivityId,
                    Title = a.Title,
                    Type = a.Types.Select(t => t.ActivityType),
                    Region = a.Regions.Select(r => r.RegionName),
                    Start = a.StartTime,
                    End = a.EndTime,
                }).ToList();

            return new PagedResponseDTO<ActivityInfoReponseDTO>(ans, q.PageNumber, q.PageSize, totalRecords);

        }



    }
}
