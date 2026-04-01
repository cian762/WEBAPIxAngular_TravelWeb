using Microsoft.EntityFrameworkCore;
using TravelWeb_API.DTO.ActivityDTO;
using TravelWeb_API.Models.ActivityModel;

namespace TravelWeb_API.Services
{
    public class ActivityCardForIndexService
    {
        private readonly ActivityDbContext _dbcontext;

        public ActivityCardForIndexService(ActivityDbContext dbcontext)
        {

            _dbcontext = dbcontext;

        }

        public async Task<List<ActivityIndexResponseDTO>> SendActivityToIndex() 
        {
            var result = await _dbcontext.Activities
                .Where(a => a.SoftDelete == false && a.EndTime > DateOnly.FromDateTime(DateTime.Today))
                .OrderBy(a => a.ViewCount)
                .Select(a => new ActivityIndexResponseDTO
                {
                    ActivityId = a.ActivityId,
                    Title = a.Title,
                    Region = a.Regions.Select(r=>r.RegionName),
                    CoverImageUrl = a.ActivityImages.Where(i=>i.IsCoverImage!=false).Select(i=>i.ImageUrl).FirstOrDefault(),
                    ViewCount = a.ViewCount,
                    AverageRating = (float)Math.Round((a.Reviews.Any() ? a.Reviews.Where(r => r.IsSoftDeleted == false).Average(r => r.Rating) : 0), 1),
                })
                .OrderByDescending(a=>a.ViewCount)
                .Take(4)
                .AsNoTracking()
                .ToListAsync();

            return result;
        }

    }
}
