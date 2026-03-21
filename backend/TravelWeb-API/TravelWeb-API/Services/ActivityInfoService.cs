using Microsoft.EntityFrameworkCore;
using TravelWeb_API.DTO.ActivityDTO;
using TravelWeb_API.Models.ActivityModel;

namespace TravelWeb_API.Services
{
    public class ActivityInfoService
    {
        private readonly ActivityDbContext _activityDbContext;
        public ActivityInfoService(ActivityDbContext activityDbContext)
        {
            _activityDbContext = activityDbContext;
        }

        public async Task<ActivityInfoResponseDTO?> GetSpecificActivityInfo(int activityId)
        {
            var check = await _activityDbContext.Activities.AnyAsync(a => a.ActivityId == activityId);

            if (!check) return null;

            var result = await _activityDbContext.Activities
                .Where(a => a.ActivityId == activityId)
                .Select(a => new ActivityInfoResponseDTO
                {
                    ActivityId = a.ActivityId,
                    Title = a.Title,
                    Regions = a.Regions.Select(a => a.RegionName).ToList(),
                    Types = a.Types.Select(a => a.ActivityType).ToList()!,
                    Description = a.Description,
                    StartTime = a.StartTime,
                    EndTime = a.EndTime,
                    Address = a.Address,
                    Longitude = a.Longitude,
                    Latitude = a.Latitude,
                    Propaganda = a.Propaganda,
                    OfficialLink = a.OfficialLink,
                    Images = a.ActivityImages.Select(a => a.ImageUrl).ToList(),
                })
                .FirstOrDefaultAsync();

            var target = await _activityDbContext.Activities
                .Where(a => a.ActivityId == activityId)
                .FirstOrDefaultAsync();

            target!.ViewCount += 1;

            await _activityDbContext.SaveChangesAsync();

            return result;
        }


        public async Task<ReviewPackageResponseDTO?> GetRelatedReviews(int activityId)
        {

            var check = await _activityDbContext.Activities.AnyAsync(a => a.ActivityId == activityId);

            if (!check) return new ReviewPackageResponseDTO()
            {
                ActivityId = activityId,
                Reviews = new List<ReviewResponseDTO>(),
                AverageRating = 0,
            };

            var reviews = await _activityDbContext.Activities
                .Where(a => a.ActivityId == activityId)
                .SelectMany(a => a.Reviews)
                .Select(r => new ReviewResponseDTO
                {
                    ReviewId = r.ReviewId,
                    MemberId = r.MemberId,
                    Title = r.Title,
                    Comment = r.Comment,
                    Rating = r.Rating,
                    CreateDate = r.CreateDate,
                    ReviewImages = r.ReviewImages.Select(i => i.ImageUrl).ToList()!
                })
                .ToListAsync();


            var averageRating = _activityDbContext.Activities
                .Where(a => a.ActivityId == activityId)
                .SelectMany(a => a.Reviews)
                .Average(r => (decimal?)r.Rating) ?? 0;

            var commentCount = _activityDbContext.Activities
                .Where(a => a.ActivityId == activityId)
                .SelectMany(a => a.Reviews)
                .Select(r => r.ReviewId)
                .Count();
                

            return new ReviewPackageResponseDTO()
            {
                ActivityId = activityId,
                Reviews = reviews,
                AverageRating = Math.Round(averageRating, 1),
                CommentCount = commentCount
            };
        }


        public async Task<List<ActivityTicketIntroResponseDTO>> GetRelatedTickets(int activityId)
        {
            var check = await _activityDbContext.Activities.AnyAsync(a => a.ActivityId == activityId);
            if (!check) return new List<ActivityTicketIntroResponseDTO>();

            var products = await _activityDbContext.Activities
                .Where(a => a.ActivityId == activityId)
                .SelectMany(a => a.ActivityTicketDetails)
                .Where(atd => atd.ProductCodeNavigation.Status != "已下架")
                .Select(atd => new ActivityTicketIntroResponseDTO
                {
                    ProductCode = atd.ProductCode,
                    ProductName = atd.ProductCodeNavigation.ProductName!,
                    CurrentPrice = atd.ProductCodeNavigation.CurrentPrice,
                    Notes = atd.Note,
                    CoverImageUrl = atd.ProductCodeNavigation.CoverImageUrl,
                    Status = atd.ProductCodeNavigation.Status
                })
                .ToListAsync();
                
                return products;
        }
    }
}
