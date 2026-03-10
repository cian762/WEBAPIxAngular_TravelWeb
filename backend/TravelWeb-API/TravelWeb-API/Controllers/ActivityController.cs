using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Linq;
using TravelWeb_API.DTO.ActivityDTO;
using TravelWeb_API.Models.ActivityModel;

namespace TravelWeb_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ActivityController : ControllerBase
    {
        private readonly ActivityDbContext _dbContext;

        private readonly IEnumerable<Activity> _activityInfo;

        public ActivityController(ActivityDbContext dbContext)
        {
            _dbContext = dbContext;
            _activityInfo = dbContext.Activities.Include(a=>a.Regions).Include(a=>a.Types).AsNoTracking().ToList();
        }


        //拿取所有活動資訊
        [HttpGet]
        public IActionResult GetActivities()
        {
            var activities = _dbContext.Activities.ToList();
            return Ok(activities);
        }

        
        //有條件的拉取活動資訊
        //填入的日期格式要是 YYYY-MM-DD
        [HttpGet("{Type?}/{Region?}/{Start?}/{End?}")]
        public ActionResult GetSpecificActivies([FromRoute] string? Type, [FromRoute] string? Region, [FromRoute] DateOnly? Start, [FromRoute] DateOnly? End) 
        {
            FilterActivityDTO filter = new FilterActivityDTO
            {
                ActivityType = Type,
                LaunchRegion = Region,
                StartTime = Start,
                EndTime = End
            };

            var result = _activityInfo
                .Where(a => a.SoftDelete == false);

            if (!filter.ActivityType.IsNullOrEmpty()) 
            {
                result = result.Where(a => a.Types.Any(t => t.ActivityType == filter.ActivityType));
            }

            if (!filter.LaunchRegion.IsNullOrEmpty()) 
            {
                result = result.Where(a => a.Regions.Any(r => r.RegionName == filter.LaunchRegion));
            }

            if (filter.StartTime.HasValue && filter.EndTime.HasValue) 
            {
                result = result.Where(a => a.StartTime >= filter.StartTime.Value && a.EndTime <= filter.EndTime.Value);
            }

            var ans = result.Select(a => new
            {
                Title = a.Title,
                Type = a.Types.Select(t => t.ActivityType),
                Region = a.Regions.Select(r => r.RegionName),
                Start = a.StartTime,
                End = a.EndTime,
            });

            return Ok(ans);
        }




    }
}
