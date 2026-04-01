using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Razor.TagHelpers;
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
        private readonly ActivityCardForIndexService _indexService;
        

        public ActivityCardController(ActivityCardService infoService, ActivityCardForIndexService indexService)
        {
            _infoService = infoService;
            _indexService = indexService;
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
        public async  Task<ActionResult> SearchByActiviteTitle([FromQuery] string searchText) 
        {
            var result = await _infoService.SearchSpecificCards(searchText);
            if (result == null) return NotFound();
            return Ok(result);
        }


        [HttpGet("ShowIndex")]
        //拉取要送去首頁的推薦
        public async Task<ActionResult> SendCardToIndex() 
        {
            var result = await _indexService.SendActivityToIndex();
            if (result == null) return NotFound();
            return Ok(result);
        }



    }
}
