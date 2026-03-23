using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using TravelWeb_API.DTO.ActivityDTO;
using TravelWeb_API.Services;

namespace TravelWeb_API.Controllers.Activity
{
    [Route("api/[controller]")]
    [ApiController]
    public class ActivityTicketController : ControllerBase
    {
        private readonly ActivityTicketService _TicketService;

        public ActivityTicketController(ActivityTicketService TicketService)
        {
            _TicketService = TicketService;
        }


        [HttpGet]
        public async Task<ActionResult<ActivityTicketInfoResponseDTO>> GetTicketInfo([FromQuery] string productCode) 
        {
            var result = await _TicketService.GetTicketInfo(productCode);
            if(result == null) return NotFound();
            return Ok(result);
        }

        [HttpGet("Suggest")]
        public async Task<ActionResult<ActivityCardReponseDTO>> GetProductSuggestion([FromQuery] int activityId,[FromQuery] List<string> ActivityType)
        {
            var result = await _TicketService.GetProductSuggestion(activityId,ActivityType);
            if (result == null) return NotFound();
            return Ok(result);
        }



    }
}
