using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Linq;
using TravelWeb_API.DTO.ActivityDTO;
using TravelWeb_API.Models.ActivityModel;
using TravelWeb_API.QueryParameters.ActivityQueryParameters;
using TravelWeb_API.Services;

namespace TravelWeb_API.Controllers.Activity
{
    [Route("api/[controller]")]
    [ApiController]
    public class ActivityCardController : ControllerBase
    {
        private readonly ActivityCardService _infoService;

        public ActivityCardController(ActivityCardService infoService)
        {
            _infoService = infoService;
        }


        //首次載入時，拿取所有活動資訊
        [HttpGet]
        public ActionResult GetActivities([FromQuery] PagedQueryParameters query)
        {
            return Ok(_infoService.GetCards(query));
        }

        
        //有條件的拉取活動資訊
        //填入的日期格式要是 YYYY-MM-DD
        [HttpGet("Query")]
        public ActionResult GetSpecificActivies([FromQuery] ActivityInfoParameters query) 
        {
            return Ok(_infoService.GetSpecificCards(query));
        }



        //有條件的拉取活動資訊，且必須包含關鍵字
        [HttpGet("Key")]
        public ActionResult SearchByActiviteTitle([FromQuery] ActivityInfoParameters query) 
        {
            return Ok(_infoService.SearchSpecificCards(query));
        }




    }
}
