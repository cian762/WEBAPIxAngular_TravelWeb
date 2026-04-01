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
        public async Task<IActionResult> GetMyProfile()
        {
            var memberCode = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(memberCode))
            {
                return Unauthorized(new { message = "無法驗證身分，請重新登入" });
            }

            var memberInfo = await _context.MemberInformations
                .Include(m => m.Followeds) 
                .Include(m => m.Followers) 
                .FirstOrDefaultAsync(m => m.MemberCode == memberCode);

            if (memberInfo == null)
            {
                return NotFound(new { message = "找不到此會員的詳細資料" });
            }

            var memberList = await _context.MemberLists.FirstOrDefaultAsync(m => m.MemberCode == memberCode);

            var blackListRecords = await _context.Blockeds
                .Where(b => b.MemberId == memberInfo.MemberId)
                .ToListAsync();

            var blockedIds = blackListRecords.Select(b => b.BlockedId).ToList();
            var blockedUsersInfo = await _context.MemberInformations
                .Where(m => blockedIds.Contains(m.MemberId))
                .ToDictionaryAsync(m => m.MemberId, m => m);

            var myComplaints = await _context.MemberComplaints
                .Where(c => c.MemberId == memberInfo.MemberId)
                .OrderByDescending(c => c.CreatedAt)
                .Select(c => new
                {
                    id = c.ComplaintId,
                    subject = c.Subject,
                    email = c.ReplyEmail,
                    date = c.CreatedAt.HasValue ? c.CreatedAt.Value.ToString("yyyy-MM-dd HH:mm") : "未知時間",
                    desc = c.Description,
                    imageUrl = c.imageUrl,
                    status = "審核中", 
                    isExpanded = false 
                })
                .ToListAsync();

            var responseData = new
            {
                memberCode = memberList?.MemberCode,
                memberId = memberInfo.MemberId,
                name = memberInfo.Name,
                avatarUrl = memberInfo.AvatarUrl,
                backgroundUrl = memberInfo.BackgroundUrl, 
                gender = memberInfo.Gender,
                birthDate = memberInfo.BirthDate?.ToString("yyyy-MM-dd"), 
                email = memberList?.Email,
                phone = memberList?.Phone,
                status = memberInfo.Status,
                followersCount = memberInfo.Followers.Count, 
                complaints = myComplaints,

                followingList = memberInfo.Followeds.Select(f => new
                {
                    memberId = f.MemberId,
                    name = f.Name,
                    avatarUrl = f.AvatarUrl
                }).ToList(),

                blackList = blackListRecords.Select(b => new
                {
                    blockedId = b.BlockedId,
                    name = blockedUsersInfo.ContainsKey(b.BlockedId) ? blockedUsersInfo[b.BlockedId].Name : "未知使用者",
                    avatarUrl = blockedUsersInfo.ContainsKey(b.BlockedId) ? blockedUsersInfo[b.BlockedId].AvatarUrl : null,
                    reason = b.Reason,
                    date = b.BlockedDate?.ToString("yyyy-MM-dd") ?? "無日期"
                }).ToList()
            };

          
            return Ok(responseData);
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

        [HttpGet("public/{memberId}")]
        [AllowAnonymous] 
        public async Task<IActionResult> GetPublicProfile(string memberId)
        {
            var myMemberCode = User.FindFirstValue(ClaimTypes.NameIdentifier);
            string? myMemberId = null;

            if (!string.IsNullOrEmpty(myMemberCode))
            {
                var myInfo = await _context.MemberInformations.FirstOrDefaultAsync(m => m.MemberCode == myMemberCode);
                if (myInfo != null)
                {
                    myMemberId = myInfo.MemberId;

                    bool isBlockedByMe = await _context.Blockeds
                        .AnyAsync(b => b.MemberId == myMemberId && b.BlockedId == memberId); // 我封鎖他

                    bool isBlockingMe = await _context.Blockeds
                        .AnyAsync(b => b.MemberId == memberId && b.BlockedId == myMemberId); // 他封鎖我

                    if (isBlockedByMe || isBlockingMe)
                    {
                        return NotFound(new { message = "找不到該名會員或您無權查看此頁面" });
                    }
                }
            }

            var targetMember = await _context.MemberInformations
                .Include(m => m.Followers)
                .FirstOrDefaultAsync(m => m.MemberId == memberId);

            if (targetMember == null)
            {
                return NotFound(new { message = "找不到該名會員" });
            }

            var publicData = new
            {
                memberId = targetMember.MemberId,
                name = targetMember.Name,
                avatarUrl = targetMember.AvatarUrl ?? "assets/default-avatar.png",
                backgroundUrl = targetMember.BackgroundUrl ?? "",
                followersCount = targetMember.Followers.Count
            };

            bool isFollowing = false;
            if (myMemberId != null)
            {
                isFollowing = targetMember.Followers.Any(f => f.MemberId == myMemberId);
            }

            return Ok(new
            {
                profile = publicData,
                isFollowing = isFollowing
            });
        }
    }
}