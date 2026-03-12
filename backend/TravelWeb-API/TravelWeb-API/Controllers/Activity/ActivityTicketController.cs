using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
        public ActionResult<ActivityTicketInfoResponseDTO> GetTicketInfo([FromQuery] string productCode) 
        {
            var result = _TicketService.GetTicketInfo(productCode);
            if(result == null) return NotFound();
            return Ok(result);
        }

        [HttpGet("Suggest")]
        public ActionResult<ActivityCardReponseDTO> GetProductSuggestion([FromQuery] string ActivityType)
        {
            var result = _TicketService.GetProductSuggestion(ActivityType);
            if (result == null) return NotFound();
            return Ok(result);
        }



    }
}
