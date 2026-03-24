using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;
using TravelWeb_API.Models.MemberSystem;
using TravelWebApi.DTOs;

namespace TravelWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // 🛡️ 整個控制器都必須登入才能存取
    public class MemberProfileController : ControllerBase
    {
        private readonly MemberSystemContext _context;
        private readonly IConfiguration _configuration;

        // 🔥 修正：建構式括號內必須加入 IConfiguration configuration
        public MemberProfileController(MemberSystemContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // ==========================================
        // 🟢 GET: api/MemberProfile/me (取得我的完整個人資料)
        // ==========================================
        [HttpGet("me")]
        public async Task<ActionResult<MemberProfileResponseDto>> GetMyProfile()
        {
            var memberCode = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(memberCode))
            {
                return Unauthorized(new { message = "無法驗證身分，請重新登入" });
            }

            // 🔥 修正：直接 Select 成你建立好的 MemberProfileResponseDto
            var userProfile = await (from list in _context.MemberLists
                                     join info in _context.MemberInformations
                                     on list.MemberCode equals info.MemberCode
                                     where list.MemberCode == memberCode
                                     select new MemberProfileResponseDto
                                     {
                                         MemberCode = list.MemberCode,
                                         MemberId = info.MemberId,
                                         Name = info.Name,
                                         AvatarUrl = info.AvatarUrl,
                                         Gender = info.Gender,
                                         BirthDate = info.BirthDate,
                                         Email = list.Email,
                                         Phone = list.Phone,
                                         Status = info.Status
                                     }).FirstOrDefaultAsync();

            if (userProfile == null)
            {
                return NotFound(new { message = "找不到此會員的詳細資料" });
            }

            return Ok(userProfile);
        }

        // ==========================================
        // 🟡 PUT: api/MemberProfile/me (更新我的個人資料含大頭貼)
        // ==========================================
        [HttpPut("me")]
        public async Task<IActionResult> UpdateMyProfile([FromForm] MemberProfileUpdateDto dto)
        {
            var memberCode = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(memberCode))
            {
                return Unauthorized(new { message = "無法驗證身分，請重新登入" });
            }

            var memberInfo = await _context.MemberInformations
                                           .FirstOrDefaultAsync(m => m.MemberCode == memberCode);

            if (memberInfo == null)
            {
                return NotFound(new { message = "找不到此會員的個資紀錄" });
            }

            // 更新姓名
            if (!string.IsNullOrWhiteSpace(dto.Name))
            {
                memberInfo.Name = dto.Name;
            }

            // ==========================================
            // 處理 Cloudinary 圖片上傳
            // ==========================================
            // 🔥 確認這裡呼叫的是 dto.AvatarFile (與下方修正的 DTO 對應)
            if (dto.AvatarFile != null && dto.AvatarFile.Length > 0)
            {
                var cloudName = _configuration["CloudinarySettings:CloudName"];
                var apiKey = _configuration["CloudinarySettings:ApiKey"];
                var apiSecret = _configuration["CloudinarySettings:ApiSecret"];

                Account account = new Account(cloudName, apiKey, apiSecret);
                Cloudinary cloudinary = new Cloudinary(account);

                using var stream = dto.AvatarFile.OpenReadStream();
                var uploadParams = new ImageUploadParams()
                {
                    File = new FileDescription(dto.AvatarFile.FileName, stream),
                    Folder = "TravelWeb/Avatars",
                    Transformation = new Transformation().Width(500).Height(500).Crop("fill").Gravity("face")
                };

                var uploadResult = await cloudinary.UploadAsync(uploadParams);

                if (uploadResult.Error != null)
                {
                    return StatusCode(500, new { message = "大頭貼上傳失敗", error = uploadResult.Error.Message });
                }

                memberInfo.AvatarUrl = uploadResult.SecureUrl.ToString();
            }

            try
            {
                await _context.SaveChangesAsync();
                return Ok(new { message = "個人資料已成功更新！", avatarUrl = memberInfo.AvatarUrl });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "更新失敗，請稍後再試。" });
            }
        }
    }
}