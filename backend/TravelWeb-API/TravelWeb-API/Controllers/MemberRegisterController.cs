using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
// 🔥 修正 1：改用 ASP.NET Core 正確的 Configuration 命名空間
using Microsoft.Extensions.Configuration;
// 🔥 修正 2：補上 JWT 產生所需的命名空間
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using TravelWeb_API.DTO.MemberSystemDto;
using TravelWeb_API.Models.MemberSystem;

namespace TravelWeb_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class MemberRegisterController : ControllerBase
    {
        private readonly MemberSystemContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly IConfiguration _configuration;

        public MemberRegisterController(MemberSystemContext context, IWebHostEnvironment env, IConfiguration configuration)
        {
            _context = context;
            _env = env;
            _configuration = configuration;
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
            // MemberId 產生規則：「信箱@前字串 + 隨機兩字 (英數混合)」
            // ==========================================
            string emailPrefix = request.Email.Split('@')[0];
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            string randomSuffix = new string(Enumerable.Repeat(chars, 2)
                .Select(s => s[random.Next(s.Length)]).ToArray());

            string newMemberId = "@"+emailPrefix + randomSuffix;

            var newMemberInfo = new MemberInformation
            {
                MemberId = newMemberId,
                MemberCode = newMemberCode,
                Name = request.Name,
                Gender = request.Gender,
                BirthDate = request.BirthDate,
                Status = "正常"
            };

            // ==========================================
            // 處理大頭貼上傳
            // ==========================================
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
                newMemberInfo.AvatarUrl = "/images/default-avatar.png";
            }

            // ==========================================
            // 寫入資料庫 (使用 Transaction 確保兩張表都寫入成功)
            // ==========================================
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                _context.MemberLists.Add(newMemberAccount);
                await _context.SaveChangesAsync();

                _context.MemberInformations.Add(newMemberInfo);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                // 註冊成功後自動登入：產生 Token 並存入 Cookie
                string role = newMemberCode.StartsWith("G") ? "Admin" : "Member";

                // 這裡呼叫了下方的 GenerateJwtToken 方法
                string token = GenerateJwtToken(newMemberCode, role, newMemberId);

                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true, // 如果你在本機沒有用 https 開發，測試時可以先改成 false，上線一定要 true
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTime.UtcNow.AddHours(9)
                };
                Response.Cookies.Append("AuthToken", token, cookieOptions);

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

        private string GenerateJwtToken(string memberCode, string role, string memberId)
        {
            var issuer = _configuration["JwtSettings:Issuer"];
            var audience = _configuration["JwtSettings:Audience"];
            var signKey = _configuration["JwtSettings:SignKey"];

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, memberCode),
                new Claim(ClaimTypes.Role, role),
                new Claim("MemberId", memberId), // 將 MemberId 封裝進 Token
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signKey));
            var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Issuer = issuer,
                Audience = audience,
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddHours(9),
                SigningCredentials = signingCredentials
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var securityToken = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(securityToken);
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
