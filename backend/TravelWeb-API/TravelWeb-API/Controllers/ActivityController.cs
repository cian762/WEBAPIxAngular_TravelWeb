using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Linq;
using TravelWeb_API.DTO.ActivityDTO;
using TravelWeb_API.Models.ActivityModel;
using TravelWeb_API.QueryParameters.ActivityQueryParameters;
using TravelWeb_API.Services;

namespace TravelWeb_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ActivityController : ControllerBase
    {
        private readonly ActivityInfoService _infoService;

        public ActivityController(ActivityInfoService infoService)
        {
            _infoService = infoService;
        }


        //拿取所有活動資訊
        [HttpGet]
        public IActionResult GetActivities([FromQuery] ActivityInfoParameters query)
        {
            return Ok(_infoService.GetActivities(query));
        }

        
        //有條件的拉取活動資訊
        //填入的日期格式要是 YYYY-MM-DD
        [HttpGet("Query")]
        public ActionResult GetSpecificActivies([FromQuery] ActivityInfoParameters query) 
        {
            return Ok(_infoService.GetSpecificActivities(query));
        }






    }
}
