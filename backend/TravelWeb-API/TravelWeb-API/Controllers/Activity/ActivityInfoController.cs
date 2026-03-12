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
        public ActionResult GetSpecificActivityInfo(int activityId)
        {
            var result = _activityInfoService.GetSpecificActivityInfo(activityId);
            return Ok(result);
        }
    }
}
