using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using TravelWeb_API.DTO.ActivityDTO;
using TravelWeb_API.Services;

namespace TravelWeb_API.Controllers.Activity
{
    [Route("api/[controller]")]
    [ApiController]
    public class ActivityInfoController : ControllerBase
    {
        private readonly ActivityInfoService _activityInfoService;
        private readonly GoogleRouteForActivityService _googleRouteForActivityService;

        public ActivityInfoController(ActivityInfoService activityInfoService, GoogleRouteForActivityService googleRouteForActivityService)
        {
            _activityInfoService = activityInfoService;
            _googleRouteForActivityService = googleRouteForActivityService;
        }


        [HttpGet("{activityId}")]
        public async Task<ActionResult> GetSpecificActivityInfo([FromRoute] int activityId)
        {
            var result = await _activityInfoService.GetSpecificActivityInfo(activityId);
            if (result == null) return NotFound();
            return Ok(result);
        }


        [HttpGet("{activitiyId}/Reviews")]
        public async Task<ActionResult> GetRelatedReviews([FromRoute] int activitiyId, [FromQuery] string orderRule)
        {
            var memberCode = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
            var result = await _activityInfoService.GetRelatedReviews(activitiyId,memberCode,orderRule);
            if (result == null) return NotFound();
            return Ok(result);
        }


        [HttpGet("{activityId}/Tickets")]
        public async Task<ActionResult> GetRelatedTickets([FromRoute] int activityId)
        {
            var result = await _activityInfoService.GetRelatedTickets(activityId);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpPost("RoutePlan")]
        public async Task<ActionResult<RouteResponseDTO>> GetRoute([FromBody] RouteRequestDTO request) 
        {
            var result = await _googleRouteForActivityService.ComputeRouteAsync(request);

            if (result == null)
            {
                return BadRequest("查無路線");
            }

            return Ok(result);

        }


        //這是文章討論那邊用來抓近期活動的        
        [HttpGet("NewActivity")]
        public IActionResult GetNewActivity()
        {
            var result = _activityInfoService.GetNewActivity();
            return Ok(result);
        }

        


    }
}
