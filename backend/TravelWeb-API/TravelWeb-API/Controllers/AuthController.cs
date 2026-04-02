using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using TravelWeb_API.DTO.MemberSystemDto;
using TravelWeb_API.Models;
using TravelWeb_API.Models.MemberSystem;
using TravelWeb_API.Services;
using TravelWeb_API.DTO.MemberSystemDto;

namespace TravelWeb_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly MemberSystemContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(MemberSystemContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            if (string.IsNullOrEmpty(request.Account) || string.IsNullOrEmpty(request.Password))
            {
                return BadRequest(new { message = "請輸入帳號與密碼" });
            }

            string hashedPassword = HashPassword(request.Password);

            var user = await _context.MemberLists
                .FirstOrDefaultAsync(x => (x.Email == request.Account || x.MemberCode == request.Account)
                                       && x.PasswordHash == hashedPassword);

            if (user == null)
            {
                return Unauthorized(new { message = "帳號或密碼錯誤" });
            }

            var info = await _context.MemberInformations.FirstOrDefaultAsync(i => i.MemberCode == user.MemberCode);
            if (info != null && info.Status == "停權")
            {
                return StatusCode(403, new { message = "您的帳號已被管理員停權，禁止登入！" });
            }

            string role = user.MemberCode.StartsWith("G") ? "Admin" : "Member";

            string memberId = info?.MemberId ?? "";
            string token = GenerateJwtToken(user.MemberCode, role, memberId);

            var cookieOptions = new CookieOptions
            {
                HttpOnly = true, 
                Secure = true,  
                SameSite = SameSiteMode.None, 
                Expires = DateTime.UtcNow.AddHours(9) 
            };
            Response.Cookies.Append("AuthToken", token, cookieOptions);

            try
            {
                var loginRecord = new LogInRecord
                {
                    MemberCode = user.MemberCode,
                    LoginAt = DateTime.Now
                };

                _context.LogInRecords.Add(loginRecord);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine("寫入登入紀錄失敗：" + ex.InnerException?.Message ?? ex.Message);
            }

            return Ok(new
            {
                message = "登入成功",
                userCode = user.MemberCode,
                role = role,
                 token = token
            });
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            Response.Cookies.Delete("AuthToken", new CookieOptions
            {
                Secure = true,
                SameSite = SameSiteMode.None,
            });

            return Ok(new { message = "已成功登出" });
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
                new Claim("MemberId", memberId),
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

        private string HashPassword(string password)
        {
            using (SHA256 sha = SHA256.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(password);
                byte[] hash = sha.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }

        [HttpGet("check-status")]
        public IActionResult CheckStatus()
        {
            var token = Request.Cookies["AuthToken"];

            if (string.IsNullOrEmpty(token))
            {
                return Ok(false); 
            }

            return Ok(true);
        }

        [HttpPost("send-verification-code")]
        public async Task<IActionResult> SendVerificationCode([FromBody] string email, [FromServices] IMemberEmailService emailService)
        {
            if (string.IsNullOrWhiteSpace(email)) return BadRequest("信箱不可為空");

            bool isAlreadyMember = await _context.MemberLists.AnyAsync(m => m.Email == email);
            if (isAlreadyMember) return Conflict(new { message = "此信箱已經註冊過會員，請直接登入！" });

            string code = new Random().Next(100000, 999999).ToString();

            var existingRecord = await _context.EmailVerifications.FirstOrDefaultAsync(e => e.Email == email);

            if (existingRecord != null)
            {
                existingRecord.VerificationCode = code;
                existingRecord.ExpiryTime = DateTime.Now.AddMinutes(10);

                existingRecord.IsVerified = false;
            }
            else
            {
                var newRecord = new EmailVerification
                {
                    Email = email,
                    VerificationCode = code,
                    ExpiryTime = DateTime.Now.AddMinutes(10),
                    IsVerified = false
                };
                _context.EmailVerifications.Add(newRecord);
            }

            await _context.SaveChangesAsync();

            try
            {
                await emailService.SendVerificationCodeAsync(email, code);
                return Ok(new { message = "驗證碼已發送至您的信箱，請查收！" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "寄信失敗，請稍後再試", error = ex.Message });
            }
        }

        public class VerifyCodeDto { public string Email { get; set; } public string Code { get; set; } }

        [HttpPost("verify-code")]
        public async Task<IActionResult> VerifyCode([FromBody] VerifyCodeDto request)
        {
            var record = await _context.EmailVerifications.FirstOrDefaultAsync(e => e.Email == request.Email);

            if (record == null)
            {
                return BadRequest(new { message = "請先發送驗證碼" });
            }

            if (DateTime.Now > record.ExpiryTime)
            {
                return BadRequest(new { message = "驗證碼已過期，請重新發送" });
            }

            if (record.VerificationCode != request.Code)
            {
                return BadRequest(new { message = "驗證碼錯誤" });
            }

            record.IsVerified = true;
            await _context.SaveChangesAsync();

            return Ok(new { message = "信箱驗證成功！" });
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] string account, [FromServices] IMemberEmailService emailService)
        {
            if (string.IsNullOrWhiteSpace(account)) return BadRequest(new { message = "請輸入信箱或會員代碼" });

            var user = await _context.MemberLists.FirstOrDefaultAsync(m => m.Email == account || m.MemberCode == account);
            if (user == null) return NotFound(new { message = "找不到此帳號，請確認輸入是否正確" });

            string code = new Random().Next(1000, 9999).ToString();

            var existingRecord = await _context.EmailVerifications.FirstOrDefaultAsync(e => e.Email == user.Email);
            if (existingRecord != null)
            {
                existingRecord.VerificationCode = code;
                existingRecord.ExpiryTime = DateTime.Now.AddMinutes(15); 
                existingRecord.IsVerified = false;
            }
            else
            {
                var newRecord = new EmailVerification
                {
                    Email = user.Email,
                    VerificationCode = code,
                    ExpiryTime = DateTime.Now.AddMinutes(15),
                    IsVerified = false
                };
                _context.EmailVerifications.Add(newRecord);
            }
            await _context.SaveChangesAsync();

            string frontendUrl = _configuration["FrontendSettings:BaseUrl"];
            string resetLink = $"{frontendUrl}/reset-password?email={user.Email}";

            await emailService.SendPasswordResetEmailAsync(user.Email, code, resetLink);

            return Ok(new { message = "重設密碼驗證信已寄出，請前往信箱查收！" });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "資料格式錯誤", errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
            }

            string cleanCode = request.Code?.Trim();

            var record = await _context.EmailVerifications.FirstOrDefaultAsync(e => e.Email == request.Email);

            if (record == null)
            {
                return BadRequest(new { message = "找不到驗證碼紀錄，請重新發送驗證信" });
            }

            if (record.VerificationCode != cleanCode || DateTime.Now > record.ExpiryTime)
            {
                return BadRequest(new { message = "驗證碼錯誤或已過期，請重新申請" });
            }

            var user = await _context.MemberLists.FirstOrDefaultAsync(m => m.Email == request.Email);
            if (user == null)
            {
                return NotFound(new { message = "找不到該名使用者的帳號資料" });
            }

            try
            {
                user.PasswordHash = HashPassword(request.NewPassword);

                record.IsVerified = true;
                record.ExpiryTime = DateTime.Now.AddDays(-1); 
                record.VerificationCode = "USED"; 

                await _context.SaveChangesAsync();

                return Ok(new { message = "密碼重設成功，請使用新密碼登入！" });
            }
            catch (Exception ex)
            {
                string realError = ex.InnerException != null ? ex.InnerException.Message : ex.Message;

                Console.WriteLine("重設密碼存檔失敗：" + realError);

                return StatusCode(500, new
                {
                    message = "資料庫寫入失敗，請聯絡管理員",
                    error = realError
                });
            }
            ;
        }
    }
}