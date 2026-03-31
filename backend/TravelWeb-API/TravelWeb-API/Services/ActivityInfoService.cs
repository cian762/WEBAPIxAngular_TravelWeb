using Microsoft.EntityFrameworkCore;
using TravelWeb_API.DTO.ActivityDTO;
using TravelWeb_API.Models.ActivityModel;
using TravelWeb_API.Models.TripProduct.TripDTO;

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
                .AsNoTracking()
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


        public async Task<ReviewPackageResponseDTO?> GetRelatedReviews(int activityId, string? memberId,string orderRule="highest")
        {

            var check = await _activityDbContext.Activities.AnyAsync(a => a.ActivityId == activityId);

            if (!check) return new ReviewPackageResponseDTO()
            {
                ActivityId = activityId,
                Reviews = new List<ReviewResponseDTO>(),
                AverageRating = 0,
            };

            var reviews = _activityDbContext.Activities
                .AsNoTracking()
                .Where(a => a.ActivityId == activityId)
                .SelectMany(a => a.Reviews)
                .Where(r => memberId == null || r.MemberId != memberId)
                .Where(r => r.IsSoftDeleted == false);

            if (orderRule == "highest")
            {
                reviews = reviews
                     .OrderByDescending(r => r.Rating)
                     .ThenByDescending(r => r.CreateDate);

            }
            else if (orderRule == "lowest")
            {
                reviews = reviews
                    .OrderBy(r => r.Rating)
                    .ThenByDescending(r => r.CreateDate);
            }
            else if (orderRule == "newest")
            {
                reviews = reviews
                    .OrderByDescending(r => r.CreateDate);
            }
            else if (orderRule == "picFirst")
            {
                reviews = reviews
                    .OrderByDescending(r => r.ReviewImages.Count())
                    .ThenByDescending(r => r.CreateDate);
            }

            var result = await reviews
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
                .AsNoTracking()
                .Where(a => a.ActivityId == activityId)
                .SelectMany(a => a.Reviews)
                .Where(r=>r.IsSoftDeleted==false)
                .Average(r => (decimal?)r.Rating) ?? 0;

            var commentCount = _activityDbContext.Activities
                .AsNoTracking()
                .Where(a => a.ActivityId == activityId)
                .SelectMany(a => a.Reviews)
                .Where(r => r.IsSoftDeleted == false && r.MemberId != memberId)
                .Select(r => r.ReviewId)
                .Count();
                

            return new ReviewPackageResponseDTO()
            {
                ActivityId = activityId,
                Reviews = result,
                AverageRating = Math.Round(averageRating, 1),
                CommentCount = commentCount
            };
        }


        public async Task<List<ActivityTicketIntroResponseDTO>> GetRelatedTickets(int activityId)
        {
            var check = await _activityDbContext.Activities.AnyAsync(a => a.ActivityId == activityId);
            if (!check) return new List<ActivityTicketIntroResponseDTO>();

            var products = await _activityDbContext.Activities
                .AsNoTracking()
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




        //GetNewActivity這是文章討論那邊用來抓近期活動的
        public List<NewActivity> GetNewActivity()
        {
            List<NewActivity> results = _activityDbContext.Activities
                .Take(10)
                .Select(a => new NewActivity { activityId= a.ActivityId, title=a.Title })
                .ToList();

            return results;
        }
        //GetNewActivity這是文章討論那邊用來抓近期活動的
        public class NewActivity {
            public int activityId { get; set; }
            public string? title { get; set; }
        }
    }
}
