using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TravelWeb_API.Models.ActivityModel;

namespace TravelWeb_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ActivityController : ControllerBase
    {
        private readonly ActivityDbContext _dbContext;

        public ActivityController(ActivityDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        public IActionResult GetActivities()
        {
            var activities = _dbContext.Activities.ToList();
            return Ok(activities);
        }

    }
}
