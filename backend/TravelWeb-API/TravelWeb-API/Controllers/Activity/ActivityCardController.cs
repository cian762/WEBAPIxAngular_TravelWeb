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
        public async Task<ActionResult> GetActivities([FromQuery] PagedQueryParameters query)
        {
            return Ok(await _infoService.GetCards(query));
        }

        
        //有條件的拉取活動資訊
        //填入的日期格式要是 YYYY-MM-DD
        [HttpGet("Query")]
        public async Task<ActionResult> GetSpecificActivies([FromQuery] ActivityInfoParameters query) 
        {
            return Ok(await _infoService.GetSpecificCards(query));
        }



        //有條件的拉取活動資訊，且必須包含關鍵字
        [HttpGet("Keyword")]
        public async  Task<ActionResult> SearchByActiviteTitle([FromQuery] string serachText) 
        {
            var result = await _infoService.SearchSpecificCards(serachText);
            if (result == null) return NotFound();
            return Ok(result);
        }




    }
}
