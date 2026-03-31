 using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
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
        private readonly IConfiguration _configuration;

        public MemberProfileController(MemberSystemContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpGet("me")]
        public async Task<ActionResult<MemberProfileResponseDto>> GetMyProfile()
        {
            var memberCode = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(memberCode))
            {
                return Unauthorized(new { message = "無法驗證身分，請重新登入" });
            }

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
                                         BackgroundUrl = info.BackgroundUrl,
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

            if (!string.IsNullOrWhiteSpace(dto.Name))
            {
                memberInfo.Name = dto.Name;
            }

            var cloudName = _configuration["CloudinarySettings:CloudName"];
            var apiKey = _configuration["CloudinarySettings:ApiKey"];
            var apiSecret = _configuration["CloudinarySettings:ApiSecret"];

            Account account = new Account(cloudName, apiKey, apiSecret);
            Cloudinary cloudinary = new Cloudinary(account); 

            if (dto.AvatarFile != null && dto.AvatarFile.Length > 0)
            {
                using var stream = dto.AvatarFile.OpenReadStream();
                var uploadParams = new ImageUploadParams()
                {
                    File = new FileDescription(dto.AvatarFile.FileName, stream),
                    Folder = "TravelWeb/Avatars",
                    Transformation = new Transformation().Width(500).Height(500).Crop("fill").Gravity("face")
                };

                var uploadResult = await cloudinary.UploadAsync(uploadParams); 

                if (uploadResult.Error != null) return StatusCode(500, new { message = "大頭貼上傳失敗" });

                memberInfo.AvatarUrl = uploadResult.SecureUrl.ToString();
            }

            if (dto.BackgroundFile != null && dto.BackgroundFile.Length > 0)
            {
                using var stream = dto.BackgroundFile.OpenReadStream();
                var uploadParams = new ImageUploadParams() 
                {
                    File = new FileDescription(dto.BackgroundFile.FileName, stream),
                    Folder = "TravelWeb/Backgrounds",
                    Transformation = new Transformation().Width(1200).Height(400).Crop("fill")
                };

                var uploadResult = await cloudinary.UploadAsync(uploadParams); 

                if (uploadResult.Error != null) return StatusCode(500, new { message = "背景圖上傳失敗" });

                memberInfo.BackgroundUrl = uploadResult.SecureUrl.ToString();
            }

            try
            {
                await _context.SaveChangesAsync();
                return Ok(new
                {
                    message = "個人資料已成功更新！",
                    avatarUrl = memberInfo.AvatarUrl,
                    backgroundUrl = memberInfo.BackgroundUrl
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "更新失敗，請稍後再試。" });
            }
        }

        //[HttpGet("public/{memberId}")]
        //[AllowAnonymous] 
        //public async Task<IActionResult> GetPublicProfile(string memberId)
        //{
        //    var targetMember = await _context.MemberInformations
        //        .Include(m => m.Followers)
        //        .FirstOrDefaultAsync(m => m.MemberId == memberId);

        //    if (targetMember == null)
        //    {
        //        return NotFound(new { message = "找不到該名會員" });
        //    }

        //    // 2. 準備要回傳的公開資料 (過濾掉信箱、電話、密碼等敏感資料)
        //    var publicData = new
        //    {
        //        memberId = targetMember.MemberId,
        //        name = targetMember.Name,
        //        avatarUrl = targetMember.AvatarUrl ?? "assets/default-avatar.png",
        //        coverUrl = targetMember.BackgroundUrl ?? "", 
        //        followersCount = targetMember.Followers.Count, 
        //    };

        //    bool isFollowing = false;

        //    var myMemberCode = User.FindFirstValue(ClaimTypes.NameIdentifier);
        //    if (!string.IsNullOrEmpty(myMemberCode))
        //    {
        //        var myInfo = await _context.MemberInformations.FirstOrDefaultAsync(m => m.MemberCode == myMemberCode);
        //        if (myInfo != null)
        //        {
        //            isFollowing = targetMember.Followers.Any(f => f.MemberId == myInfo.MemberId);
        //        }
        //    }

        //    return Ok(new
        //    {
        //        profile = publicData,
        //        isFollowing = isFollowing
        //    });
        //}
    }
}