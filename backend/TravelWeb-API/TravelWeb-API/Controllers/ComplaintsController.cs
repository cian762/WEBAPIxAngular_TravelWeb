using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration; // 🔥 引入 IConfiguration
using System.Security.Claims;
using TravelWeb_API.Models.MemberSystem;
using TravelWebApi.DTOs; // 請確保這是你 DTO 的正確命名空間

namespace TravelWeb_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // 必須登入
    public class ComplaintController : ControllerBase
    {
        private readonly MemberSystemContext _context;
        private readonly IConfiguration _configuration; // 🔥 改為注入設定檔

        public ComplaintController(MemberSystemContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
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

            // ==========================================
            // 🖼️ 處理圖片上傳 (Cloudinary)
            // ==========================================
            if (request.ImageFile != null && request.ImageFile.Length > 0)
            {
                // 1. 讀取 Cloudinary 金鑰
                var cloudName = _configuration["CloudinarySettings:CloudName"];
                var apiKey = _configuration["CloudinarySettings:ApiKey"];
                var apiSecret = _configuration["CloudinarySettings:ApiSecret"];

                Account account = new Account(cloudName, apiKey, apiSecret);
                Cloudinary cloudinary = new Cloudinary(account);

                // 2. 設定上傳參數
                using var stream = request.ImageFile.OpenReadStream();
                var uploadParams = new ImageUploadParams()
                {
                    File = new FileDescription(request.ImageFile.FileName, stream),
                    Folder = "TravelWeb/Complaints", // 🔥 建議建一個專屬資料夾放客訴圖片
                    // 客訴圖片通常需要保留原圖讓管理員看清楚，所以這裡不加上 Crop 裁切
                };

                // 3. 執行上傳
                var uploadResult = await cloudinary.UploadAsync(uploadParams);

                if (uploadResult.Error != null)
                {
                    return StatusCode(500, new { message = "圖片上傳失敗", error = uploadResult.Error.Message });
                }

                // 4. 將 Cloudinary 回傳的網址存入資料庫模型
                // ⚠️ 注意：C# 的 Model 屬性通常是首字母大寫，請確認你的 MemberComplaint 裡面是 ImageUrl 還是 imageUrl
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