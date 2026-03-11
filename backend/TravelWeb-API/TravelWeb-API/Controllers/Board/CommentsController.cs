using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TravelWeb_API.Models.Board;
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
        private readonly IArticlesService _ArticlesService;

        public CommentsController(BoardDbContext context,
            IArticlesService noteService,
            MemberSystemContext memberDb)
        {
            _context = context;
            _ArticlesService = noteService;
            _memberDb = memberDb;
        }

        // Get:根據文章ID抓對應留言
        [HttpGet("{id}")]
        public async Task<ActionResult<List<CommentsDTO>>> GetComments(int id)
        {
            var comments = _ArticlesService.GetComments(id);
            if (comments == null) return NotFound();
            return comments;
        }

        // POST:新增留言
        [HttpPost]
        public async Task<ActionResult<Comment>> PostComment(int articleID, string UserId, string contents)
        {
            Comment comment = _ArticlesService.AddComment(articleID, UserId, contents);            
            
            return NoContent();
        }
    }
}
