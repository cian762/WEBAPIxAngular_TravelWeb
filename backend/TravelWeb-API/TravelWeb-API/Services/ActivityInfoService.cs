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
                    Images = a.ActivityImages.Select(a=> a.ImageUrl).ToList(),
                })
                .FirstOrDefaultAsync();
            return result;
        }
    }
}
