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
    public class ComplaintsController : ControllerBase
    {
        private readonly MemberSystemContext _context;

        public ComplaintsController(MemberSystemContext context)
        {
            _context = context;
        }


        [HttpPost]
        public async Task<IActionResult> CreateComplaint([FromBody] ComplaintCreateDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var memberIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                                          ?? User.FindFirst("MemberId")?.Value;

            if (string.IsNullOrEmpty(memberIdString))
            {
                return Unauthorized(new { message = "無法取得登入者身分，請重新登入" });
            }
            string memberId = memberIdString;

            DateTime now = DateTime.Now;
            string datePrefix = now.ToString("yyyyMMdd");

            var lastComplaint = await _context.MemberComplaints
                .Where(c => c.ComplaintId.StartsWith(datePrefix))
                .OrderByDescending(c => c.ComplaintId)
                .FirstOrDefaultAsync();

            int nextSequence = 1;

            if (lastComplaint != null)
            {
                string lastSequenceStr = lastComplaint.ComplaintId.Substring(8);
                if (int.TryParse(lastSequenceStr, out int lastSeq))
                {
                    nextSequence = lastSeq + 1;
                }
            }

            string newComplaintId = $"{datePrefix}{nextSequence:D3}";

            var newComplaint = new MemberComplaint
            {
                ComplaintId = newComplaintId,
                MemberId = memberId,
                Description = dto.Description,
                ReplyEmail = dto.ReplyEmail,
                CreatedAt = now
            };

            _context.MemberComplaints.Add(newComplaint);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                message = "投訴信已成功送出！",
                complaintId = newComplaintId
            });
        }
    }
}