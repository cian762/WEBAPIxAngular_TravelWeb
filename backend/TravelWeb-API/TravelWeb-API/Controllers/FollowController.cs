using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TravelWeb_API.DTO.MemberSystemDto;
using TravelWeb_API.Models.MemberSystem;

namespace TravelWeb_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] 
    public class FollowController : ControllerBase
    {
        private readonly MemberSystemContext _context;

        public FollowController(MemberSystemContext context)
        {
            _context = context;
        }

        [HttpPost("toggle")]
        public async Task<IActionResult> ToggleFollow([FromBody] FollowRequestDto request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            string myMemberCode = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var myInfo = await _context.MemberInformations
                .Include(m => m.Followeds)
                .FirstOrDefaultAsync(m => m.MemberCode == myMemberCode);

            if (myInfo == null) return Unauthorized(new { message = "登入狀態異常" });

            if (myInfo.MemberId == request.FollowedId)
            {
                return BadRequest(new { message = "您不能追隨自己喔！" });
            }

            var targetMember = await _context.MemberInformations
                .FirstOrDefaultAsync(m => m.MemberId == request.FollowedId);

            if (targetMember == null)
            {
                return NotFound(new { message = "找不到該名會員" });
            }

            bool isAlreadyFollowing = myInfo.Followeds.Any(f => f.MemberId == request.FollowedId);

            if (isAlreadyFollowing)
            {
                myInfo.Followeds.Remove(targetMember);
                await _context.SaveChangesAsync();

                return Ok(new { message = $"已取消追隨 {targetMember.Name}" });
            }
            else
            {
                myInfo.Followeds.Add(targetMember);
                await _context.SaveChangesAsync();

                return Ok(new { message = $"成功追隨了 {targetMember.Name}！" });
            }
        }
    }
}