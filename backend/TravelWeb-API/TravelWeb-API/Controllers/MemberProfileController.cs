using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TravelWeb_API.Models.MemberSystem;
using TravelWebApi.DTOs;

namespace TravelWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class MemberProfileController : ControllerBase
    {
        private readonly MemberSystemContext _context;

        public MemberProfileController(MemberSystemContext context)
        {
            _context = context;
        }

        [HttpGet("me")]

        public async Task<IActionResult> GetMyProfile()
        {
            var memberCode = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                          ?? User.FindFirst("MemberCode")?.Value;

            if (string.IsNullOrEmpty(memberCode))
            {
                return Unauthorized(new { success = false, message = "無法驗證身分，請重新登入" });
            }

            var userProfile = await (from list in _context.MemberLists
                                     join info in _context.MemberInformations
                                     on list.MemberCode equals info.MemberCode
                                     where list.MemberCode == memberCode
                                     select new MemberProfileResponseDto
                                     {
                                         MemberId = info.MemberId.ToString(),
                                         Name = info.Name,
                                         AvatarUrl = info.AvatarUrl,
                                         Gender = info.Gender == 1 ? "男" : (info.Gender == 2 ? "女" : "未填寫"),
                                         BirthDate = info.BirthDate,
                                         Email = list.Email,
                                         Phone = list.Phone
                                     }).FirstOrDefaultAsync();

            if (userProfile == null)
            {
                return NotFound(new { success = false, message = "找不到此會員的詳細資料" });
            }

            return Ok(new
            {
                success = true,
                data = userProfile
            });


        }

        // PUT: api/MemberProfile/me
        [HttpPut("me")]
        public async Task<IActionResult> UpdateMyProfile([FromBody] MemberProfileUpdateDto dto)
        {
            // ==========================================
            // 1. 從 JWT Token 中取得目前登入使用者的 MemberCode
            // ==========================================
            var memberCode = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                          ?? User.FindFirst("MemberCode")?.Value;

            if (string.IsNullOrEmpty(memberCode))
            {
                return Unauthorized(new { success = false, message = "無法驗證身分，請重新登入" });
            }

            // ==========================================
            // 2. 去資料庫找出該名會員的「詳細個資 (Member_Information)」
            // ==========================================
            var memberInfo = await _context.MemberInformations
                                           .FirstOrDefaultAsync(m => m.MemberCode == memberCode);

            if (memberInfo == null)
            {
                return NotFound(new { success = false, message = "找不到此會員的個資紀錄" });
            }

            // ==========================================
            // 3. 更新允許修改的欄位 (Name, AvatarUrl)
            // ==========================================
            // 將前端傳來的資料 (dto) 覆蓋掉資料庫原本的資料 (memberInfo)
            memberInfo.Name = dto.Name;
            memberInfo.AvatarUrl = dto.AvatarUrl;

            // (注意：因為我們沒有開放 Gender 或 BirthDate，所以在這裡絕對不會動到它們，非常安全！)

            // ==========================================
            // 4. 儲存變更至資料庫
            // ==========================================
            try
            {
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "個人資料已成功更新！"
                });
            }
            catch (Exception ex)
            {
                // 捕捉資料庫儲存時可能發生的例外錯誤
                return StatusCode(500, new { success = false, message = "更新失敗，請稍後再試。" });
            }
        }
    }
}