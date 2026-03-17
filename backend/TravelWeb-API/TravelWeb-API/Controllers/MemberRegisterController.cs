using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using TravelWeb_API.Models.MemberSystem;
using TravelWeb_API.DTO.MemberSystemDto;

namespace TravelWeb_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class MemberRegisterController : ControllerBase
    {
        private readonly MemberSystemContext _context;
        private readonly IWebHostEnvironment _env; 

        public MemberRegisterController(MemberSystemContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        [HttpPost]
        public async Task<IActionResult> Register([FromForm] RegisterRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new { message = "Email 與 密碼為必填欄位" });
            }

            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return BadRequest(new { message = "姓名為必填欄位" });
            }

            bool emailExists = await _context.MemberLists.AnyAsync(m => m.Email == request.Email);
            if (emailExists)
            {
                return Conflict(new { message = "此 Email 已經被註冊過了" });
            }

            string newMemberCode = "M" + DateTime.Now.ToString("yyyyMMddHHmmss") + new Random().Next(100, 999);

            var newMemberAccount = new MemberList
            {
                MemberCode = newMemberCode,
                Email = request.Email,
                Phone = request.Phone,
                PasswordHash = HashPassword(request.Password)
            };

            // ==========================================
            // 🔥 更新：MemberId 產生規則改為「信箱@前字串 + 隨機兩字 (英數混合)」
            // ==========================================
            string emailPrefix = request.Email.Split('@')[0];
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            string randomSuffix = new string(Enumerable.Repeat(chars, 2)
                .Select(s => s[random.Next(s.Length)]).ToArray());

            string newMemberId = emailPrefix + randomSuffix;

            var newMemberInfo = new MemberInformation
            {
                MemberId = newMemberId,
                MemberCode = newMemberCode,
                Name = request.Name,
                Gender = request.Gender,
                BirthDate = request.BirthDate,
                Status = "正常"
            };

            if (request.AvatarFile != null && request.AvatarFile.Length > 0)
            {
                string uploadsFolder = Path.Combine(_env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "uploads");

                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(request.AvatarFile.FileName);
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await request.AvatarFile.CopyToAsync(stream);
                }

                newMemberInfo.AvatarUrl = "/uploads/" + uniqueFileName;
            }
            else
            {
                // 如果沒有上傳圖片，塞入預設灰人圖片路徑
                newMemberInfo.AvatarUrl = "/images/default-avatar.png";
            }

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                _context.MemberLists.Add(newMemberAccount);
                await _context.SaveChangesAsync();

                _context.MemberInformations.Add(newMemberInfo);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return Ok(new
                {
                    message = "會員註冊成功！",
                    memberCode = newMemberCode,
                    memberId = newMemberId 
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                return StatusCode(500, new { message = "資料庫寫入失敗", error = ex.InnerException?.Message ?? ex.Message });
            }
        }

        // ==========================================
        // 密碼加密方法 (SHA256)
        // ==========================================
        private string HashPassword(string password)
        {
            using (SHA256 sha = SHA256.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(password);
                byte[] hash = sha.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }
    }
}
