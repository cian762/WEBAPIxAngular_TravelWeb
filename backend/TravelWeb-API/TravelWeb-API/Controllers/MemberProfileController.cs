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
                                         //BackgroundUrl = info.BackgroundUrl,
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
            // 🔥 關鍵修正：把初始化 Cloudinary 放在這裡 (兩個 if 的外面)
            // ==========================================
            var cloudName = _configuration["CloudinarySettings:CloudName"];
            var apiKey = _configuration["CloudinarySettings:ApiKey"];
            var apiSecret = _configuration["CloudinarySettings:ApiSecret"];

            Account account = new Account(cloudName, apiKey, apiSecret);
            Cloudinary cloudinary = new Cloudinary(account); // 宣告在這裡，下面兩個 if 都能用！

            // ==========================================
            // 處理大頭貼上傳
            // ==========================================
            if (dto.AvatarFile != null && dto.AvatarFile.Length > 0)
            {
                using var stream = dto.AvatarFile.OpenReadStream();
                var uploadParams = new ImageUploadParams()
                {
                    File = new FileDescription(dto.AvatarFile.FileName, stream),
                    Folder = "TravelWeb/Avatars",
                    Transformation = new Transformation().Width(500).Height(500).Crop("fill").Gravity("face")
                };

                var uploadResult = await cloudinary.UploadAsync(uploadParams); // 這裡可以用

                if (uploadResult.Error != null) return StatusCode(500, new { message = "大頭貼上傳失敗" });

                memberInfo.AvatarUrl = uploadResult.SecureUrl.ToString();
            }

            // ==========================================
            // 處理背景圖上傳
            // ==========================================
            if (dto.BackgroundFile != null && dto.BackgroundFile.Length > 0)
            {
                using var stream = dto.BackgroundFile.OpenReadStream();
                var uploadParams = new ImageUploadParams() // ⚠️注意：這裡不要寫成 var uploadParams = ... 如果上面宣告過會衝突，建議大頭貼的叫 avatarParams，背景的叫 bgParams，或者直接用 { } 區塊隔開 (如現在的寫法)
                {
                    File = new FileDescription(dto.BackgroundFile.FileName, stream),
                    Folder = "TravelWeb/Backgrounds",
                    Transformation = new Transformation().Width(1200).Height(400).Crop("fill")
                };

                var uploadResult = await cloudinary.UploadAsync(uploadParams); // 這裡也可以用了！

                if (uploadResult.Error != null) return StatusCode(500, new { message = "背景圖上傳失敗" });

                //memberInfo.BackgroundUrl = uploadResult.SecureUrl.ToString();
            }

            // ==========================================
            // 寫入資料庫並回傳
            // ==========================================
            try
            {
                await _context.SaveChangesAsync();
                return Ok(new
                {
                    message = "個人資料已成功更新！",
                    avatarUrl = memberInfo.AvatarUrl,
                    //backgroundUrl = memberInfo.BackgroundUrl
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "更新失敗，請稍後再試。" });
            }
        }
    }
}