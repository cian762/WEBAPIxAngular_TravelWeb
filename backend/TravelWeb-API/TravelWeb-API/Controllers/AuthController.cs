using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using TravelWeb_API.Models;
using TravelWeb_API.Models.MemberSystem;
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

            // 5. 判斷角色 (G 開頭是管理員 Admin，M 開頭是會員 Member)
            string role = user.MemberCode.StartsWith("G") ? "Admin" : "Member";

            string token = GenerateJwtToken(user.MemberCode, role);

            // 7. 登入成功，將 Token 與基本資訊回傳給前端
            return Ok(new
            {
                message = "登入成功",
                token = token,
                userCode = user.MemberCode,
                role = role
            });
        }

        private string GenerateJwtToken(string memberCode, string role)
        {
            var issuer = _configuration["JwtSettings:Issuer"];
            var audience = _configuration["JwtSettings:Audience"];
            var signKey = _configuration["JwtSettings:SignKey"];

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, memberCode), 
                new Claim(ClaimTypes.Role, role),  
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