using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

[ApiController]
[Authorize] // 🔥 有這個標籤，系統就會自動去 Cookie 驗證並解開 Token
public class SomeActionController : ControllerBase
{
    [HttpGet("my-action")]
    public IActionResult DoSomething()
    { 
        // 🔥 直接讀取存放在 Cookie (Token 內) 的 MemberCode
        string memberCode = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        // 🔥 直接讀取存放在 Cookie (Token 內) 的 MemberId
        string memberId = User.FindFirst("MemberId")?.Value;

        // 🔥 讀取角色
        string role = User.FindFirst(ClaimTypes.Role)?.Value;

        return Ok(new
        {
            message = $"嗨！{memberCode}，你的身分證(ID)是 {memberId}，執行動作成功！"
        });
    }
}