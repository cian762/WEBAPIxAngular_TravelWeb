using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TravelWeb_API.DTO.MemberSystemDto;
using TravelWeb_API.Models.MemberSystem;
using TravelWebApi.DTOs; // 替換為您的 Models 命名空間

namespace TravelWeb_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // 🛡️ 必須登入才能投訴！
    public class ComplaintController : ControllerBase
    {
        private readonly MemberSystemContext _context;

        public ComplaintController(MemberSystemContext context)
        {
            _context = context;
        }

        [HttpPost("submit")]
        public async Task<IActionResult> SubmitComplaint([FromBody] ComplaintCreateDto request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // 1. 從 Token 中抓出登入者的 MemberCode
            string myMemberCode = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // 透過 MemberCode 找出他的 MemberId (寫入 Member_Complaint 需要用到)
            var myInfo = await _context.MemberInformations.FirstOrDefaultAsync(m => m.MemberCode == myMemberCode);
            if (myInfo == null) return Unauthorized(new { message = "找不到會員資料" });

            // 2. 自動產生 ComplaintId (格式：yyyyMMdd + 3碼流水號，如 20231027001)
            string todayPrefix = DateTime.Now.ToString("yyyyMMdd");
            var lastRecord = await _context.ComplaintRecords
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

            // 3. 準備資料
            // 主表：Complaint_Record
            var complaintRecord = new ComplaintRecord
            {
                ComplaintId = newComplaintId,
                MemberCode = myMemberCode,
                Status = "已檢視" // 預設狀態
            };

            // 副表：Member_Complaint (明細)
            var memberComplaint = new MemberComplaint
            {
                ComplaintId = newComplaintId,
                MemberId = myInfo.MemberId,
                Description = request.Description,
                ReplyEmail = request.ReplyEmail,
                CreatedAt = DateTime.Now
            };

            // 4. 使用 Transaction 一口氣寫入兩張表
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _context.ComplaintRecords.Add(complaintRecord);
                await _context.SaveChangesAsync();

                _context.MemberComplaints.Add(memberComplaint);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return Ok(new
                {
                    message = "投訴信已成功送出",
                    complaintId = newComplaintId
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { message = "系統發生錯誤，投訴失敗", error = ex.Message });
            }
        }
    }
}