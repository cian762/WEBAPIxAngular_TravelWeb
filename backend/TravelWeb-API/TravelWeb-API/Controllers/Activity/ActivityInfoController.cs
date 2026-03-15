using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TravelWeb_API.Services;

namespace TravelWeb_API.Controllers.Activity
{
    [Route("api/[controller]")]
    [ApiController]
    public class ActivityInfoController : ControllerBase
    {
        private readonly ActivityInfoService _activityInfoService;
        public ActivityInfoController(ActivityInfoService activityInfoService)
        {
            _activityInfoService = activityInfoService;
        }


        [HttpGet]
        public async Task<ActionResult> GetSpecificActivityInfo(int activityId)
        {
            var result = await _activityInfoService.GetSpecificActivityInfo(activityId);
            if (result == null) return NotFound();
            return Ok(result);
        }
    }
}
