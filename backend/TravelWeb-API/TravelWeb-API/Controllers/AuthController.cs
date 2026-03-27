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

            // 5. 判斷角色 (G 開頭是管理員 Admin，M 開頭是會員 Member)
            string role = user.MemberCode.StartsWith("G") ? "Admin" : "Member";

            // 🔥 1. 產生 Token 時，把 MemberId 也傳進去
            string memberId = info?.MemberId ?? "";
            string token = GenerateJwtToken(user.MemberCode, role, memberId);

            // 🔥 2. 將 Token 存入 HttpOnly Cookie 中
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true, // 防止 XSS 攻擊 (前端 JS 讀不到)
                Secure = true,   // 限制只能在 HTTPS 環境下傳輸
                SameSite = SameSiteMode.None, // 防止 CSRF 跨站攻擊
                Expires = DateTime.UtcNow.AddHours(9) // 與 Token 過期時間一致
            };
            Response.Cookies.Append("AuthToken", token, cookieOptions);

            // ==========================================
            // 🚀 3. 關鍵修復：寫入登入紀錄 (Log_in_record)
            // ==========================================
            try
            {
                // 🔥 就是這麼簡單！我們「只給」MemberCode 和 LoginAt
                // 絕對不要寫 LoginRecordId = xxx！
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
                // 萬一出錯，把真實原因印出來看
                Console.WriteLine("🚨 寫入登入紀錄失敗：" + ex.InnerException?.Message ?? ex.Message);
            }

            return Ok(new
            {
                message = "登入成功",
                userCode = user.MemberCode,
                role = role,
                 token = token
                // (可選) 既然存 Cookie 了，這裡可以不用回傳明文 Token，看前端需求
            });
        }

        // 🔥 3. 新增登出 API (用來清除 Cookie)
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

        // 🔥 4. 修改產生 Token 的方法，多接收一個 memberId 參數
        private string GenerateJwtToken(string memberCode, string role, string memberId)
        {
            var issuer = _configuration["JwtSettings:Issuer"];
            var audience = _configuration["JwtSettings:Audience"];
            var signKey = _configuration["JwtSettings:SignKey"];

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, memberCode),
                new Claim(ClaimTypes.Role, role),
                new Claim("MemberId", memberId), // 🔥 把 MemberId 封裝進 Token
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
        //檢查登入狀態勿刪搭配路由守門員使用
        [HttpGet("check-status")]
        public IActionResult CheckStatus()
        {
            // 檢查名為 "AuthToken" 的 Cookie 是否存在
            var token = Request.Cookies["AuthToken"];

            if (string.IsNullOrEmpty(token))
            {
                return Ok(false); // 沒 Cookie，代表沒登入
            }

            // 進階：你也可以在這裡驗證 JWT Token 是否過期
            // 如果只是初步練習，檢查有無字串即可
            return Ok(true);
        }

        // ==========================================
        // 📧 POST: api/Auth/send-verification-code (發送驗證碼)
        // ==========================================
        [HttpPost("send-verification-code")]
        public async Task<IActionResult> SendVerificationCode([FromBody] string email, [FromServices] IMemberEmailService emailService)
        {
            if (string.IsNullOrWhiteSpace(email)) return BadRequest("信箱不可為空");

            // 1. 檢查信箱是否已經註冊過會員了？
            bool isAlreadyMember = await _context.MemberLists.AnyAsync(m => m.Email == email);
            if (isAlreadyMember) return Conflict(new { message = "此信箱已經註冊過會員，請直接登入！" });

            // 2. 產生一組隨機 6 位數驗證碼
            string code = new Random().Next(100000, 999999).ToString();

            // 3. 尋找暫存表是否已有紀錄
            var existingRecord = await _context.EmailVerifications.FirstOrDefaultAsync(e => e.Email == email);

            if (existingRecord != null)
            {
                // 如果有紀錄，更新驗證碼與到期時間 (10分鐘)
                existingRecord.VerificationCode = code;
                existingRecord.ExpiryTime = DateTime.Now.AddMinutes(10);

                // 🔥 確保有強制設回 false
                existingRecord.IsVerified = false;
            }
            else
            {
                // 如果沒紀錄，新增一筆
                var newRecord = new EmailVerification
                {
                    Email = email,
                    VerificationCode = code,
                    ExpiryTime = DateTime.Now.AddMinutes(10),

                    // 🔥 確保有強制設為 false
                    IsVerified = false
                };
                _context.EmailVerifications.Add(newRecord);
            }

            // 這一步執行時，資料庫就不會再報 NULL 錯誤了！
            await _context.SaveChangesAsync();

            // 4. 呼叫 EmailService 把驗證碼寄出去
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

        // ==========================================
        // 🔐 POST: api/Auth/verify-code (比對前端輸入的驗證碼)
        // ==========================================
        public class VerifyCodeDto { public string Email { get; set; } public string Code { get; set; } }

        [HttpPost("verify-code")]
        public async Task<IActionResult> VerifyCode([FromBody] VerifyCodeDto request)
        {
            var record = await _context.EmailVerifications.FirstOrDefaultAsync(e => e.Email == request.Email);

            if (record == null)
            {
                return BadRequest(new { message = "請先發送驗證碼" });
            }

            // 檢查是否過期
            if (DateTime.Now > record.ExpiryTime)
            {
                return BadRequest(new { message = "驗證碼已過期，請重新發送" });
            }

            // 比對驗證碼
            if (record.VerificationCode != request.Code)
            {
                return BadRequest(new { message = "驗證碼錯誤" });
            }

            // 驗證成功！將狀態改為 true
            record.IsVerified = true;
            await _context.SaveChangesAsync();

            return Ok(new { message = "信箱驗證成功！" });
        }


    }
}