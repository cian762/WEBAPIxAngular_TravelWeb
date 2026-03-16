using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using TravelWeb_API.Models.MemberSystem; // 👈 請確認您的 Models 命名空間

namespace TravelWeb_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    // ⚠️ 註冊 API 必須是公開的，任何人都可以呼叫，所以加上 [AllowAnonymous]
    [AllowAnonymous]
    public class MemberRegisterController : ControllerBase
    {
        private readonly MemberSystemContext _context;
        private readonly IWebHostEnvironment _env; // 用來取得伺服器實體路徑 (存圖片用)

        public MemberRegisterController(MemberSystemContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // ==========================================
        // 📦 定義前端傳來的資料格式 (DTO)
        // 因為有檔案上傳，等一下 API 必須使用 multipart/form-data
        // ==========================================
        public class RegisterRequestDto
        {
            // --- Member_List 需求欄位 ---
            public string Email { get; set; }
            public string Password { get; set; }
            public string Phone { get; set; }

            // --- Member_Information 需求欄位 ---
            public string Name { get; set; }

            // 1=男, 2=女
            public byte? Gender { get; set; }

            // 注意：配合您之前資料庫的型別，這裡使用 DateOnly?
            public DateOnly? BirthDate { get; set; }

            // 接收前端上傳的圖片檔案
            public IFormFile? AvatarFile { get; set; }
        }

        // ==========================================
        // 🚀 POST: api/MemberRegister (執行會員註冊)
        // ==========================================
        [HttpPost]
        public async Task<IActionResult> Register([FromForm] RegisterRequestDto request)
        {
            // 1. 基本防呆與必填檢查
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new { message = "Email 與 密碼為必填欄位" });
            }

            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return BadRequest(new { message = "姓名為必填欄位" });
            }

            // 2. 檢查 Email 是否已被註冊過
            bool emailExists = await _context.MemberLists.AnyAsync(m => m.Email == request.Email);
            if (emailExists)
            {
                // 回傳 409 Conflict 代表資料衝突
                return Conflict(new { message = "此 Email 已經被註冊過了" });
            }

            // ---------------------------------------------------
            // 🛠️ 準備第一張表資料：Member_List
            // ---------------------------------------------------

            // 自動產生 MemberCode (規則：M + yyyyMMddHHmmss + 3碼亂數)
            string newMemberCode = "M" + DateTime.Now.ToString("yyyyMMddHHmmss") + new Random().Next(100, 999);

            var newMemberAccount = new MemberList
            {
                MemberCode = newMemberCode,
                Email = request.Email,
                Phone = request.Phone,
                PasswordHash = HashPassword(request.Password) // 密碼加密
            };

            // ---------------------------------------------------
            // 🛠️ 準備第二張表資料：Member_Information
            // ---------------------------------------------------

            // 自動產生 MemberId (規則：Email @ 前面的字串 + 3碼亂數)
            string emailPrefix = request.Email.Split('@')[0];
            string newMemberId = emailPrefix + new Random().Next(100, 999);

            var newMemberInfo = new MemberInformation
            {
                MemberId = newMemberId,
                MemberCode = newMemberCode, // 🔗 關聯到剛剛產生的帳號代碼
                Name = request.Name,
                Gender = request.Gender,
                BirthDate = request.BirthDate,
                Status = "正常" // 預設狀態
            };

            // 🖼️ 處理大頭貼檔案上傳
            if (request.AvatarFile != null && request.AvatarFile.Length > 0)
            {
                // 取得 wwwroot/uploads 的實體路徑
                string uploadsFolder = Path.Combine(_env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "uploads");

                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                // 產生唯一檔名，避免重複覆蓋
                string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(request.AvatarFile.FileName);
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                // 將檔案存入伺服器
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

            // ---------------------------------------------------
            // 💾 寫入資料庫 (利用 Transaction 確保兩張表同時成功)
            // ---------------------------------------------------

            // ---------------------------------------------------
            // 💾 寫入資料庫 (利用 Transaction 確保兩張表同時成功)
            // ---------------------------------------------------

            // 🔥 建立一個資料庫交易，確保如果中間出錯，全部資料都會倒退 (Rollback)
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // 🚀 第一步：【先】將主表 (Member_List) 存入資料庫
                _context.MemberLists.Add(newMemberAccount);
                await _context.SaveChangesAsync();
                // 此時，MemberCode 已經真實存在資料庫中了！

                // 🚀 第二步：【再】將副表 (Member_Information) 存入資料庫
                _context.MemberInformations.Add(newMemberInfo);
                await _context.SaveChangesAsync();
                // 因為此時爸爸已經在了，兒子絕對可以順利建立！

                // 🎉 第三步：兩步都成功了，正式確認提交 (Commit) 到資料庫！
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
                // 💥 如果在第一步或第二步發生任何錯誤 (例如有人在此瞬間搶走了一樣的 Email)
                // 交易機制會發揮作用，自動把已經寫入的資料復原，保證資料庫的乾淨！
                await transaction.RollbackAsync();

                // 回傳詳細的錯誤訊息方便除錯
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
