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
using TravelWeb_API.Models.Board.IService;
using TravelWeb_API.Models.MemberSystem;

namespace TravelWeb_API.Controllers.Board
{
    [Route("api/Board/[controller]")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "Board")]
    public class CommentsController : ControllerBase
    {
        private readonly BoardDbContext _context;
        private readonly MemberSystemContext _memberDb;
        // private readonly IArticlesService _ArticlesService;
        private readonly ICommentsService _commentsService;

        public CommentsController(BoardDbContext context,
            ICommentsService commentsService,
            MemberSystemContext memberDb)
        {
            _context = context;
            _commentsService = commentsService;
            _memberDb = memberDb;
        }

        // Get:根據文章ID抓對應留言
        [HttpGet("{id}")]
        public async Task<ActionResult<List<CommentsDTO>>> GetComments(int id)
        {
            var comments = _commentsService.GetComments(id);
            if (comments == null) return NotFound();
            return comments;
        }

        // POST:新增留言
        [HttpPost("PostComment")]
        public async Task<ActionResult<Comment>> PostComment([FromBody] PostCommentDto dto)
        {
            // 從 Cookie 取出 Token  
            string? token = Request.Cookies["AuthToken"];
            string? currentUserId = GetUser.Id(token);
            Comment comment = _commentsService.AddComment(dto, currentUserId);

            return NoContent();
        }

        



        // Put:點讚機制
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCommentLike(int commentID)
        {
            // 從 Cookie 取出 Token  
            string? token = Request.Cookies["AuthToken"];
            string? currentUserId = GetUser.Id(token);
            if (currentUserId == null) return NotFound();
            bool isUserExist = _commentsService.CommentLike(commentID, currentUserId);
            if (isUserExist) return NoContent();//只要有成功就改UI，或者再看看
            return NotFound();
        }

        // POST:刪除留言
        [HttpDelete("DeleteComment")]
        public async Task<IActionResult> DeleteComment(int commentID)
        {
            // 從 Cookie 取出 Token  
            // 從 Cookie 取出 Token  
            string? token = Request.Cookies["AuthToken"];
            string? currentUserId = GetUser.Id(token);
            if (currentUserId==null) return NotFound();
            try
            {
                await _commentsService.DeleteComment(commentID, currentUserId);
                return NoContent();
            }
            catch
            {
                return NotFound();
            }
        }
    }
}
