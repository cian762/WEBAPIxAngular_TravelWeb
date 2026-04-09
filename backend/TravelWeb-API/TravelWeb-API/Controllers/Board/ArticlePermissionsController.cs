using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using TravelWeb_API.Models.Board.DbSet;
using TravelWeb_API.Models.Board.DTO;
using TravelWeb_API.Models.MemberSystem;

namespace TravelWeb_API.Controllers.Board
{
    [Route("api/Board/[controller]")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "Board")]
    public class ArticlePermissionsController : ControllerBase
    {
        private readonly MemberSystemContext _context;
        private readonly BoardDbContext _boardDbContext;
        public ArticlePermissionsController(MemberSystemContext context,
            BoardDbContext boardDbContext)
        {
            _context = context;
            _boardDbContext = boardDbContext;
        }

        [HttpGet("isFollowing")]
        public async Task<ActionResult<bool>> Follow(string followedId)
        {
            // 從 Cookie 取出 Token  
            string? token = Request.Cookies["AuthToken"];
            string? currentUserId = GetUser.Id(token);
            if (currentUserId == null) return false;
            var isFollowing = await _context.MemberInformations
                .Where(m => m.MemberId == currentUserId)
                .AnyAsync(m => m.Followeds.Any(f => f.MemberId == followedId));
            return isFollowing;


        }
        //   [HttpPost("Follow")]
        //   public async Task<AcceptedResult> Follow(string FollowID)
        //   {
        //       string? currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        //       if (currentUserId == null) return NotFound();
        //       var member = await _context.MemberInformations
        //.Include(m => m.Followeds)
        //.FirstOrDefaultAsync(m => m.MemberId == id);

        //       var followedList = member.Followeds;
        //       return Ok();

        //   }
        [HttpGet("myFollow")]
        public async Task<ActionResult<List<AuthorInfo>>> GetMyFollow()
        {
            // 從 Cookie 取出 Token  
            string? token = Request.Cookies["AuthToken"];
            string? currentUserId = GetUser.Id(token);
            if (currentUserId == null) return NotFound();
            var myFollow = await _context.MemberInformations
                .Where(m => m.MemberId == currentUserId)
                .SelectMany(m => m.Followeds
                .Select(f => new AuthorInfo
                {
                    authorId = f.MemberId,
                    authorName = f.Name,
                    avatarUrl = f.AvatarUrl,
                    ArticleCount = 0,
                    isCurrentUser = false
                }
                )).ToListAsync();

            foreach (var f in myFollow)
            {
                f.ArticleCount = _boardDbContext.Articles
                    .Count(a => a.UserId == f.authorId && a.Status == 1);
            }

            return myFollow;

        }

        [HttpGet("searchAuthors")]
        public async Task<ActionResult<List<AuthorInfo>>> searchAuthors(string keyword)
        {
            // 從 Cookie 取出 Token  
            string? token = Request.Cookies["AuthToken"];
            string? currentUserId = GetUser.Id(token);
            if (currentUserId == null) return NotFound();
            var result = await _context.MemberInformations
                .Where(m => m.MemberId.Contains(keyword) || m.Name.Contains(keyword))
                .Where(m => m.MemberId != currentUserId) // 排除自己
                //.Where(m => !m..Any(b => b.BlockedId == currentUserId)) // 排除封鎖我的人
                //.Where(m => !m.BlockedBys.Any(b => b.MemberId == currentUserId)) // 排除我封鎖的人
                //&& m.blockedIngs.Contains(currentUserId))
                .Select(f => new AuthorInfo
                 {
                     authorId = f.MemberId,
                     authorName = f.Name,
                     avatarUrl = f.AvatarUrl,
                     ArticleCount = 0,
                     isCurrentUser = false
                 }
                ).ToListAsync();

            return result;
        }

    }
}
