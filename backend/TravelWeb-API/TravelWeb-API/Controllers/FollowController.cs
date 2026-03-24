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
    [Authorize] // 🛡️ 必須登入才能追隨別人！
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

            // 1. 從 Token 抓出自己的 MemberCode，並轉換成自己的 MemberId
            string myMemberCode = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // 注意這裡：使用 Include 把「我正在追隨的人 (Followeds)」一併抓出來！
            var myInfo = await _context.MemberInformations
                .Include(m => m.Followeds)
                .FirstOrDefaultAsync(m => m.MemberCode == myMemberCode);

            if (myInfo == null) return Unauthorized(new { message = "登入狀態異常" });

            // 2. 防呆：不能追隨自己
            if (myInfo.MemberId == request.FollowedId)
            {
                return BadRequest(new { message = "您不能追隨自己喔！" });
            }

            // 3. 尋找對方是否存在
            var targetMember = await _context.MemberInformations
                .FirstOrDefaultAsync(m => m.MemberId == request.FollowedId);

            if (targetMember == null)
            {
                return NotFound(new { message = "找不到該名會員" });
            }

            // 4. 判斷動作：如果已經追隨了就「取消追隨」，還沒追隨就「追隨」
            bool isAlreadyFollowing = myInfo.Followeds.Any(f => f.MemberId == request.FollowedId);

            if (isAlreadyFollowing)
            {
                // 取消追隨 (EF Core 會自動去刪除 Member_Following 中介表的那一筆紀錄)
                myInfo.Followeds.Remove(targetMember);
                await _context.SaveChangesAsync();

                return Ok(new { message = $"已取消追隨 {targetMember.Name}" });
            }
            else
            {
                // 新增追隨 (EF Core 會自動去新增 Member_Following 中介表紀錄)
                myInfo.Followeds.Add(targetMember);
                await _context.SaveChangesAsync();

                return Ok(new { message = $"成功追隨了 {targetMember.Name}！" });
            }
        }
    }
}