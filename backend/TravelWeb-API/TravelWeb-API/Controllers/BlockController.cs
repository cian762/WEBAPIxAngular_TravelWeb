using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TravelWeb_API.DTO.MemberSystemDto;
using TravelWeb_API.Models.MemberSystem; // 替換為您的 Models 命名空間

namespace TravelWeb_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // 🛡️ 必須登入才能封鎖別人！
    public class BlockController : ControllerBase
    {
        private readonly MemberSystemContext _context;

        public BlockController(MemberSystemContext context)
        {
            _context = context;
        }

        // ==========================================
        // POST: api/Block/toggle (封鎖 / 解除封鎖)
        // ==========================================
        [HttpPost("toggle")]
        public async Task<IActionResult> ToggleBlock([FromBody] BlockRequestDto request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // 1. 從 Token 抓出自己的 MemberCode
            string myMemberCode = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // 透過 MemberCode 找出自己的 MemberId (需要用到 Include 來檢查有沒有追蹤他)
            var myInfo = await _context.MemberInformations
                .Include(m => m.Followeds)
                .FirstOrDefaultAsync(m => m.MemberCode == myMemberCode);

            if (myInfo == null) return Unauthorized(new { message = "登入狀態異常，找不到您的資料" });

            // 2. 防呆：不能封鎖自己
            if (myInfo.MemberId == request.BlockedId)
            {
                return BadRequest(new { message = "您不能封鎖自己喔！" });
            }

            // 3. 尋找想封鎖的對象是否存在
            var targetMember = await _context.MemberInformations
                .FirstOrDefaultAsync(m => m.MemberId == request.BlockedId);

            if (targetMember == null)
            {
                return NotFound(new { message = "找不到該名會員，可能已被刪除" });
            }

            // 4. 檢查「封鎖名單 (Blocked 表)」中，是否已經有這筆紀錄了？
            // 條件：發動封鎖的人是我 (MemberId)，被封鎖的人是他 (BlockedId)
            var existingBlock = await _context.Blockeds
                .FirstOrDefaultAsync(b => b.MemberId == myInfo.MemberId && b.BlockedId == targetMember.MemberId);

            if (existingBlock != null)
            {
                // ==========================================
                // 🟢 已經封鎖過 -> 執行「解除封鎖」
                // ==========================================
                _context.Blockeds.Remove(existingBlock);
                await _context.SaveChangesAsync();

                return Ok(new { message = $"已解除對 {targetMember.Name} 的封鎖" });
            }
            else
            {
                // ==========================================
                // 🔴 還沒封鎖過 -> 執行「封鎖」
                // ==========================================
                var newBlock = new Blocked
                {
                    MemberId = myInfo.MemberId,             // 我
                    BlockedId = targetMember.MemberId,      // 衰鬼
                    Reason = request.Reason ?? "無",         // 原因 (若無則填預設字)
                    // 備註：請確認您的欄位名稱是 Blocked 還是 BlockedDate，若是 Date 型別請指派 DateTime.Now
                    // BlockedDate = DateTime.Now 
                };

                _context.Blockeds.Add(newBlock);

                // 🔥 加碼邏輯：既然都封鎖了，當然要自動「取消追隨」他！
                // 檢查我的追隨名單 (Followeds) 裡面有沒有他
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
        // (附贈) GET: 查詢我的黑名單列表
        // ==========================================
        [HttpGet("my-blacklist")]
        public async Task<IActionResult> GetMyBlacklist()
        {
            string myMemberCode = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var myInfo = await _context.MemberInformations.FirstOrDefaultAsync(m => m.MemberCode == myMemberCode);

            if (myInfo == null) return Unauthorized();

            // 去 Blocked 表撈出所有 MemberId 是我的紀錄
            var blacklist = await _context.Blockeds
                .Where(b => b.MemberId == myInfo.MemberId)
                .Select(b => new
                {
                    blockedId = b.BlockedId,
                    reason = b.Reason
                    // 若有時間欄位可以加上 blockedDate = b.BlockedDate
                })
                .ToListAsync();

            return Ok(blacklist);
        }
    }
}