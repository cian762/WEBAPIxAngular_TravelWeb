using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TravelWeb_API.Services;

namespace TravelWeb_API.Controllers.Activity
{
    [Route("api/[controller]")]
    [ApiController]
    public class QrCodeController : ControllerBase
    {
        private readonly QRCodeService _qrCodeService;
        public QrCodeController(QRCodeService qrCodeService)
        {
            _qrCodeService = qrCodeService;
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
    }
}
