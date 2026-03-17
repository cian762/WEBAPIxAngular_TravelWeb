using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TravelWeb_API.Models.MemberSystem;

namespace TravelWeb_API.Controllers
{
    [Authorize]
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

        [Authorize]
        [HttpGet("GetAllMembers")]
        public async Task<IActionResult> GetAllMembers()
        {
            var allMembers = await _logger.MemberInformations.ToListAsync();
            return Ok(allMembers);
        }

        // 沒登入也可以看 (註冊、公開公告)
        [AllowAnonymous]
        [HttpGet("PublicInfo")]
        public IActionResult PublicInfo() { return Ok("這是公開資訊"); }

        [HttpGet("PrivateInfo")]
        public IActionResult PrivateInfo() { return Ok("這是私密資訊"); }

      
        [Authorize(Roles = "Admin")] 
        [HttpGet("AdminOnly")]
        public IActionResult GetAdminSecret()
        {
            return Ok(new { message = "老闆好！只有您能看到這些機密營收資料。" });
        }

        [Authorize]
        [HttpGet("MyProfile")]
        public async Task<IActionResult> GetMyProfile()
        {
            string myMemberCode = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(myMemberCode))
            {
                return Unauthorized(new { message = "無法從 Token 中識別您的身分" });
            }

            var myInfo = await _logger.MemberInformations
                .FirstOrDefaultAsync(m => m.MemberCode == myMemberCode);

            if (myInfo == null)
            {
                return NotFound(new { message = "找不到您的個人資料" });
            }

            return Ok(new
            {
                memberId = myInfo.MemberId,
                name = myInfo.Name,
                status = myInfo.Status,
                avatarUrl = myInfo.AvatarUrl
            });
        }

    }
}
