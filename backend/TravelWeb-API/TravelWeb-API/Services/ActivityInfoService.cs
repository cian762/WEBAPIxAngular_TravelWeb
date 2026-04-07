using Microsoft.EntityFrameworkCore;
using TravelWeb_API.DTO.ActivityDTO;
using TravelWeb_API.Models.ActivityModel;
using TravelWeb_API.Models.MemberSystem;
using TravelWeb_API.Models.TripProduct.TripDTO;

namespace TravelWeb_API.Services
{
    public class ActivityInfoService
    {
        private readonly ActivityDbContext _activityDbContext;
        private readonly MemberSystemContext _memberDbContext;
        public ActivityInfoService(ActivityDbContext activityDbContext, MemberSystemContext memberSystemContext)
        {
            _activityDbContext = activityDbContext;
            _memberDbContext = memberSystemContext;
        }

        public async Task<ActivityInfoResponseDTO?> GetSpecificActivityInfo(int activityId)
        {
            var check = await _activityDbContext.Activities.AnyAsync(a => a.ActivityId == activityId);

            if (!check) return null;

            var result = await _activityDbContext.Activities
                .AsNoTracking()
                .Include(a=>a.Reviews)
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
                    CommentCount = a.Reviews.Where(r=>r.IsSoftDeleted == false).Count(),
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

            // 1. 先從 Activity 撈出評論 (假設 reviews 已經從 _activityDbContext 拿到了)
            // 如果還沒拿，建議先 .ToList() 確保資料進到記憶體，方便後續比對
            var reviewList = reviews.ToList();

            // 2. 收集所有評論中出現過的 MemberId (去重複，減少查詢負擔)
            var memberIds = reviewList.Select(r => r.MemberId).Distinct().ToList();

            // 3. 去 MemberDbContext 撈出這些人的資料，並轉成 Dictionary
            // Key 是 MemberCode, Value 是包含名字與大頭貼的物件
            var memberLookup = (from m in _memberDbContext.MemberInformations
                                where memberIds.Contains(m.MemberCode)
                                select new { m.MemberCode, m.Name, m.AvatarUrl })
                               .ToDictionary(m => m.MemberCode, m => m);

            // 4. 最後用 Select 把兩者揉在一起
            List<ReviewResponseDTO> result = reviewList.Select(r =>
            {
                // 從 Dictionary 找人，找不到就給預設值
                memberLookup.TryGetValue(r.MemberId, out var m);

                return new ReviewResponseDTO
                {
                    ReviewId = r.ReviewId,
                    MemberId = m?.Name ?? "無名旅客", // 這裡你原本把名字塞進 MemberId 欄位
                    MemberAvatar = m?.AvatarUrl ?? "",
                    Title = r.Title,
                    Comment = r.Comment,
                    Rating = r.Rating,
                    CreateDate = r.CreateDate,
                    ReviewImages = r.ReviewImages?.Select(i => i.ImageUrl).ToList() ?? new List<string>()
                };
            })
            .ToList();



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
                .Where(a => a.EndTime >= DateOnly.FromDateTime(DateTime.Now))
                .Select(a => new NewActivity { activityId= a.ActivityId, title=a.Title})
                .Take(5)
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
