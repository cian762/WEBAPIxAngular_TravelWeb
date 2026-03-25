using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TravelWeb_API.Models.Itinerary.Service;
using TravelWeb_API.Models.MemberSystem;
// 🔥 確保引入您的 Cloudinary 服務
using TravelWeb_API.Services;
using TravelWebApi.DTOs;

namespace TravelWeb_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // 必須登入
    public class ComplaintController : ControllerBase
    {
        private readonly MemberSystemContext _context;
        private readonly ICloudinaryService _cloudinaryService; // 注入上傳服務

        public ComplaintController(MemberSystemContext context, ICloudinaryService cloudinaryService)
        {
            _context = context;
            _cloudinaryService = cloudinaryService;
        }

        // 🔥 為了接收圖片，改用 [FromForm]
        [HttpPost("submit")]
        public async Task<IActionResult> SubmitComplaint([FromForm] ComplaintCreateDto request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            string myMemberCode = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var myInfo = await _context.MemberInformations.FirstOrDefaultAsync(m => m.MemberCode == myMemberCode);
            if (myInfo == null) return Unauthorized(new { message = "找不到會員資料" });

            // 自動產生 ComplaintId (yyyyMMdd + 3碼流水號)
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

            // 準備寫入明細表
            var memberComplaint = new MemberComplaint
            {
                ComplaintId = newComplaintId,
                MemberId = myInfo.MemberId,
                Subject = request.Subject,
                Description = request.Description,
                ReplyEmail = request.ReplyEmail,
                CreatedAt = DateTime.Now
            };

            // 🖼️ 處理圖片上傳 (如果有附圖)
            if (request.ImageFile != null && request.ImageFile.Length > 0)
            {
                try
                {
                    var uploadResult = await _cloudinaryService.UploadImageAsync(request.ImageFile);
                    memberComplaint.imageUrl = uploadResult.SecureUrl.ToString();
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new { message = "圖片上傳失敗", error = ex.Message });
                }
            }

            try
            {
                // 🔥 現在我們只寫入一張表，不需要 Transaction 了！
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