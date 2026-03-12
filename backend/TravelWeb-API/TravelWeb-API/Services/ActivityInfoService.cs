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

        public ActivityInfoResponseDTO? GetSpecificActivityInfo(int activityId) 
        {
            var check = _activityDbContext.Activities.Any(a => a.ActivityId == activityId);

            if (!check) return null;

            var result = _activityDbContext.Activities
                .Where(a=> a.ActivityId == activityId)
                .Select(a=> new ActivityInfoResponseDTO 
                {
                    Title = a.Title,
                    Types = a.Types.Select(a=> a.ActivityType).ToList()!,
                    Description = a.Description,
                    StartTime = a.StartTime,
                    EndTime = a.EndTime,
                    Address = a.Address,
                    OfficialLink = a.OfficialLink,
                    //用LINQ的Select方法從ActivityImages集合中選取ImageUrl屬性，並將結果轉換為List<string>
                    Images = a.ActivityImages.Select(a=> a.ImageUrl).ToList(),
                })
                .FirstOrDefault();
            return result;
        }
    }
}
