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
                SameSite = SameSiteMode.Strict, // 防止 CSRF 跨站攻擊
                Expires = DateTime.UtcNow.AddHours(9) // 與 Token 過期時間一致
            };
            Response.Cookies.Append("AuthToken", token, cookieOptions);

            return Ok(new
            {
                message = "登入成功",
                userCode = user.MemberCode,
                role = role
                // token = token // (可選) 既然存 Cookie 了，這裡可以不用回傳明文 Token，看前端需求
            });
        }

        // 🔥 3. 新增登出 API (用來清除 Cookie)
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            Response.Cookies.Delete("AuthToken");
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
    }
}