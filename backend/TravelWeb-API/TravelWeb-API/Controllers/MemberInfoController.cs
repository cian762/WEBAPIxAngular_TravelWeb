using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TravelWeb_API.Models.MemberSystem;

namespace TravelWeb_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MemberInfoController : ControllerBase
    {
        private readonly MemberSystemContext _logger;
        public    MemberInfoController(MemberSystemContext logger)
        {
            _logger = logger;
        }
        [HttpGet]
        public IActionResult Get()
        {
            var memberInfos = _logger.MemberInformations.FirstOrDefault();
            return Ok(memberInfos);
        }

    }
}
