using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TravelWeb_API.Services;

namespace TravelWeb_API.Controllers.Activity
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmailController : ControllerBase
    {
        private readonly EmailService _emailService;

        public EmailController(EmailService emailService)
        {
            _emailService = emailService;
        }

        [HttpPost("send-order-ticket/{orderId}")]
        public async Task<IActionResult> SendOrderTicket([FromRoute] int orderId)
        {
            await _emailService.SendOrderTicketEmailAsync(orderId);
            return Ok(new { message = "票券郵件寄送成功" });
        }
    }
}
