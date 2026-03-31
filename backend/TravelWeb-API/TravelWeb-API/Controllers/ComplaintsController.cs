using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration; 
using System.Security.Claims;
using TravelWeb_API.Models.MemberSystem;
using TravelWebApi.DTOs;

namespace TravelWeb_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ComplaintController : ControllerBase
    {
        private readonly MemberSystemContext _context;
        private readonly IConfiguration _configuration;

        public ComplaintController(MemberSystemContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("submit")]
        public async Task<IActionResult> SubmitComplaint([FromForm] ComplaintCreateDto request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            string myMemberCode = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var myInfo = await _context.MemberInformations.FirstOrDefaultAsync(m => m.MemberCode == myMemberCode);
            if (myInfo == null) return Unauthorized(new { message = "找不到會員資料" });

            string todayPrefix = DateTime.Now.ToString("yyyyMMdd");
            var lastRecord = await _context.MemberComplaints
                .Where(c => c.ComplaintId.StartsWith(todayPrefix))
                .OrderByDescending(c => c.ComplaintId)
                .FirstOrDefaultAsync();

            int sequence = 1;
            if (lastRecord != null && lastRecord.ComplaintId.Length >= 11)
            {
                if (int.TryParse(lastRecord.ComplaintId.Substring(8, 3), out int lastSeq))
                {
                    sequence = lastSeq + 1;
                }
            }
            string newComplaintId = $"{todayPrefix}{sequence:D3}";

            var memberComplaint = new MemberComplaint
            {
                ComplaintId = newComplaintId,
                MemberId = myInfo.MemberId,
                Subject = request.Subject,
                Description = request.Description,
                ReplyEmail = request.ReplyEmail,
                CreatedAt = DateTime.Now
            };

            if (request.ImageFile != null && request.ImageFile.Length > 0)
            {
                var cloudName = _configuration["CloudinarySettings:CloudName"];
                var apiKey = _configuration["CloudinarySettings:ApiKey"];
                var apiSecret = _configuration["CloudinarySettings:ApiSecret"];

                Account account = new Account(cloudName, apiKey, apiSecret);
                Cloudinary cloudinary = new Cloudinary(account);

                using var stream = request.ImageFile.OpenReadStream();
                var uploadParams = new ImageUploadParams()
                {
                    File = new FileDescription(request.ImageFile.FileName, stream),
                    Folder = "TravelWeb/Complaints", 
                };

                var uploadResult = await cloudinary.UploadAsync(uploadParams);

                if (uploadResult.Error != null)
                {
                    return StatusCode(500, new { message = "圖片上傳失敗", error = uploadResult.Error.Message });
                }
                memberComplaint.imageUrl = uploadResult.SecureUrl.ToString();
            }

            try
            {
                _context.MemberComplaints.Add(memberComplaint);
                await _context.SaveChangesAsync();

                return Ok(new { message = "投訴信已成功送出", complaintId = newComplaintId });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "系統發生錯誤，投訴失敗", error = ex.InnerException?.Message ?? ex.Message });
            }
        }
    }
}