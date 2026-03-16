using Microsoft.AspNetCore.Mvc;

namespace TravelWeb_API.Controllers // 請確認這裡的名字跟您的專案一樣
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