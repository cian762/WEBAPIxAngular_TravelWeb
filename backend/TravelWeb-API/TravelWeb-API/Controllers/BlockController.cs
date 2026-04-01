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
    public class BlockController : ControllerBase
    {
        private readonly MemberSystemContext _context;

        public BlockController(MemberSystemContext context)
        {
            _context = context;
        }

        [HttpPost("toggle")]
        public async Task<IActionResult> ToggleBlock([FromBody] BlockRequestDto request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            string myMemberCode = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var myInfo = await _context.MemberInformations
                .Include(m => m.Followeds)
                .FirstOrDefaultAsync(m => m.MemberCode == myMemberCode);

            if (myInfo == null) return Unauthorized(new { message = "登入狀態異常，找不到您的資料" });

            if (myInfo.MemberId == request.BlockedId)
            {
                return BadRequest(new { message = "您不能封鎖自己喔！" });
            }

            var targetMember = await _context.MemberInformations
                .FirstOrDefaultAsync(m => m.MemberId == request.BlockedId);

            if (targetMember == null)
            {
                return NotFound(new { message = "找不到該名會員，可能已被刪除" });
            }

            var existingBlock = await _context.Blockeds
                .FirstOrDefaultAsync(b => b.MemberId == myInfo.MemberId && b.BlockedId == targetMember.MemberId);

            if (existingBlock != null)
            {
                _context.Blockeds.Remove(existingBlock);
                await _context.SaveChangesAsync();

                return Ok(new { message = $"已解除對 {targetMember.Name} 的封鎖" });
            }
            else
            {
                var newBlock = new Blocked
                {
                    MemberId = myInfo.MemberId, 
                    BlockedId = targetMember.MemberId, 
                    Reason = request.Reason ?? "無",
                };

                _context.Blockeds.Add(newBlock);

                bool isFollowing = myInfo.Followeds.Any(f => f.MemberId == targetMember.MemberId);
                if (isFollowing)
                {
                    myInfo.Followeds.Remove(targetMember);
                }

                await _context.SaveChangesAsync();

                return Ok(new { message = $"成功封鎖了 {targetMember.Name}！並且已自動取消追蹤。" });
            }
        } 

        // ==========================================
        [HttpGet("my-blacklist")]
        public async Task<IActionResult> GetMyBlacklist()
        {
            string myMemberCode = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var myInfo = await _context.MemberInformations.FirstOrDefaultAsync(m => m.MemberCode == myMemberCode);

            if (myInfo == null) return Unauthorized();

            var blacklist = await _context.Blockeds
                .Where(b => b.MemberId == myInfo.MemberId)
                .Select(b => new
                {
                    blockedId = b.BlockedId,
                    reason = b.Reason
                })
                .ToListAsync();

            return Ok(blacklist);
        }
    }
}