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
                Console.WriteLine("🚨 寫入登入紀錄失敗：" + ex.InnerException?.Message ?? ex.Message);
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

        // ==========================================
        // 📧 POST: api/Auth/forgot-password (發送重設密碼驗證碼)
        // ==========================================
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] string account, [FromServices] IMemberEmailService emailService)
        {
            if (string.IsNullOrWhiteSpace(account)) return BadRequest(new { message = "請輸入信箱或會員代碼" });

            // 1. 尋找使用者是否存在
            var user = await _context.MemberLists.FirstOrDefaultAsync(m => m.Email == account || m.MemberCode == account);
            if (user == null) return NotFound(new { message = "找不到此帳號，請確認輸入是否正確" });

            // 2. 產生 4 位數驗證碼 (重設密碼專用)
            string code = new Random().Next(1000, 9999).ToString();

            // 3. 借用 Email_Verification 表來暫存驗證碼
            var existingRecord = await _context.EmailVerifications.FirstOrDefaultAsync(e => e.Email == user.Email);
            if (existingRecord != null)
            {
                existingRecord.VerificationCode = code;
                existingRecord.ExpiryTime = DateTime.Now.AddMinutes(15); // 15分鐘有效
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

            // 4. 寄出重設密碼信件 (包含驗證碼與專屬連結)
            // ⚠️ 這裡的 localhost:4200 請確認是您 Angular 的網址
            string resetLink = $"http://localhost:4200/reset-password?email={user.Email}";
            await emailService.SendPasswordResetEmailAsync(user.Email, code, resetLink);

            return Ok(new { message = "重設密碼驗證信已寄出，請前往信箱查收！" });
        }

        // ==========================================
        // 🔐 POST: api/Auth/reset-password (執行重設密碼)
        // ==========================================
        [HttpPost("reset-password")]
        // 🔥 關鍵修正：這裡必須使用剛建好的 ResetPasswordRequestDto 來接資料！
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDto request)
        {
            // 0. 防呆：先檢查前端傳來的資料格式對不對
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "資料格式錯誤", errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
            }

            // 防呆：去除前端傳來可能夾帶的空白
            string cleanCode = request.Code?.Trim();

            // 1. 去 EmailVerifications 尋找這筆驗證碼紀錄
            var record = await _context.EmailVerifications.FirstOrDefaultAsync(e => e.Email == request.Email);

            if (record == null)
            {
                return BadRequest(new { message = "找不到驗證碼紀錄，請重新發送驗證信" });
            }

            // 檢查驗證碼是否正確與過期
            if (record.VerificationCode != cleanCode || DateTime.Now > record.ExpiryTime)
            {
                return BadRequest(new { message = "驗證碼錯誤或已過期，請重新申請" });
            }

            // 2. 尋找使用者並更新密碼
            var user = await _context.MemberLists.FirstOrDefaultAsync(m => m.Email == request.Email);
            if (user == null)
            {
                return NotFound(new { message = "找不到該名使用者的帳號資料" });
            }

            try
            {
                // 執行密碼雜湊加密，覆蓋舊密碼
                user.PasswordHash = HashPassword(request.NewPassword);

                // 3. 驗證成功後，清除暫存表的紀錄，確保安全性
                _context.EmailVerifications.Remove(record);

                // 🔥 加上 Try-Catch 攔截存檔時可能發生的 500 錯誤！
                await _context.SaveChangesAsync();

                return Ok(new { message = "密碼重設成功，請使用新密碼登入！" });
            }
            catch (Exception ex)
            {
                // 剝洋蔥抓出最深層的真實錯誤原因 (SQL 報錯)
                string realError = ex.InnerException != null ? ex.InnerException.Message : ex.Message;

                // 在後端 Console 印出紅字方便除錯
                Console.WriteLine("🚨 重設密碼存檔失敗：" + realError);

                // 把真實原因回傳給前端 Angular
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