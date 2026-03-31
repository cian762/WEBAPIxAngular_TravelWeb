using Microsoft.AspNetCore.Mvc;

namespace TravelWeb_API.Controllers 
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestAuthController : ControllerBase
    {
        [HttpPost("login")]
        public IActionResult Login()
        {
            return Ok(new { message = "如果看到我，代表 API 架構沒問題！" });
        }
    }
}