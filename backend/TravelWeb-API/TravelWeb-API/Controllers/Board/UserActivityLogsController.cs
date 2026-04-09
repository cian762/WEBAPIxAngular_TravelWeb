using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using TravelWeb_API.Models.Board.DbSet;

namespace TravelWeb_API.Controllers.Board
{
    [Route("api/Board/[controller]")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "Board")]
    public class UserActivityLogsController : ControllerBase
    {
        private readonly BoardDbContext _context;

        public UserActivityLogsController(BoardDbContext context)
        {
            _context = context;
        }

        [HttpPost("{id}/view")]
        public async Task<IActionResult> LogView(int id)
        {
            // 從 Cookie 取出 Token  
            string? token = Request.Cookies["AuthToken"];
            string? currentUserId = GetUser.Id(token);
            if (currentUserId == null) return NoContent();

            // 防刷：同用戶/IP 同文章 30 分鐘內只記一次
            var since = DateTime.UtcNow.AddMinutes(-30);
            bool exists;

           
                exists = await _context.UserActivityLogs.AnyAsync(l =>
                    l.UserId == currentUserId &&
                    l.TargetId == id &&
                    l.CreatedAt >= since);
            

            if (!exists)
            {
                _context.UserActivityLogs.Add(new UserActivityLog
                {
                    UserId = currentUserId,                    
                    TargetId = id,
                    ActionType = 1,   // View
                    CreatedAt = DateTime.UtcNow
                });
                await _context.SaveChangesAsync();
            }

            return NoContent();
        }
    }
}
