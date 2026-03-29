using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TravelWeb_API.DTO.ActivityDTO;
using TravelWeb_API.Services;

namespace TravelWeb_API.Controllers.Activity
{
    [Route("api/[controller]")]
    [ApiController]
    public class QrCodeController : ControllerBase
    {
        private readonly QRCodeService _qrCodeService;
        private readonly TicketService _ticketService;
        public QrCodeController(QRCodeService qrCodeService, TicketService ticketService)
        {
            _qrCodeService = qrCodeService;
            _ticketService = ticketService;
        }

        [HttpGet("test-image")]
        public IActionResult GetTestQrImage() 
        {
            string token = _qrCodeService.GenerateToken();
            string url = _qrCodeService.BuildVerifyUrl(token);
            byte[] pngBytes = _qrCodeService.GenerateQrPngBytes(url);

            return File(pngBytes, "image/png");
        }

        [HttpGet("test-base64")]
        public IActionResult GetTestQrBase64() 
        {
            string token = _qrCodeService.GenerateToken();
            string url = _qrCodeService.BuildVerifyUrl(token);
            string base64 = _qrCodeService.GenerateQrBase64(url);

            return Ok(new
            {
                token,
                url,
                base64
            });
        }


        [HttpGet("verify")]
        public IActionResult RedirectToFrontEnd([FromQuery] QrCodeRequestDTO request)
        {
            return Redirect($"http://localhost:4200/qrCode/{request.Token}");
        }


        [HttpPost("verify")]
        public async Task<IActionResult> Validate([FromBody] QrCodeRequestDTO request) 
        {
            var memberCode = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
            var result = await _ticketService.ValidateQrCodeAsync(request.Token, memberCode);
            return Ok(result);
        }

        [HttpPost("redeem")]
        public async Task<IActionResult> Redeem([FromBody] QrCodeRequestDTO request) 
        {
            var memberCode = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
            var result = await _ticketService.RedeemQrCodeAsync(request.Token, memberCode);
            return Ok(result);
        
        }

    }
}
