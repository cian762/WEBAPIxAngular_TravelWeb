using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using TravelWeb_API.Models.MemberSystem;

namespace TravelWeb_API.Controllers.Board
{
    [Route("api/Board/[controller]")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "Board")]
    public class ArticlePermissionsController : ControllerBase
    {
        private readonly MemberSystemContext _context;

        public ArticlePermissionsController(MemberSystemContext context)
        {
            _context = context;
        }

        [HttpGet("isFollowing")]
        public async Task<ActionResult<bool>> Follow(string followedId)
        {
            string? currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (currentUserId == null) return false;
            var isFollowing = await _context.MemberInformations
                .Where(m=>m.MemberId== currentUserId)
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

    }
}
