using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TravelWeb_API.Models.Board.DbSet;
using TravelWeb_API.Models.Board.DTO;
using TravelWeb_API.Models.Board.IService;
using TravelWeb_API.Models.MemberSystem;

namespace TravelWeb_API.Controllers.Board
{
    [Route("api/[controller]")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "Board")]
    public class PostController : ControllerBase
    {
        private readonly BoardDbContext _context;
        private readonly MemberSystemContext _memberDb;
        private readonly IArticlesService _ArticlesService;

        public PostController(BoardDbContext context,
            IArticlesService noteService,
            MemberSystemContext memberDb)
        {
            _context = context;
            _ArticlesService = noteService;
            _memberDb = memberDb;
        }

        // GET: api/PostsDetailed 瀏覽Post
        [HttpGet("{id}")]
        public async Task<ActionResult<PostDetailDto>> GetPostDetail(Article article)
        {
            PostDetailDto postDetail =
                _ArticlesService.GetPostDetailed(article);
            if (postDetail == null) return NotFound();
            return postDetail;
        }

        [HttpPost]
        public async Task<IActionResult> PostPost(Article article)
        {
            if (article.Type == 0)
            {
                _ArticlesService.AddPost(article);
                return NoContent();
            }
            else if (article.Type == 1)
            {
                return NotFound();
            }
            else
            {
                return BadRequest();
            }
            
        }

        // PUT: api/Articles/5 修改文章
        [HttpPut("{id}")]
        public async Task<IActionResult> PutArticle(
            int id, string? Title, string? PhotoUrl, byte Status,
            string? content, int? regionId,
            List<string>? photoUrlList)
        {
            //修改快速串文
            byte type = _context.Articles.FirstOrDefault(a => a.ArticleId == id).Type;
            bool isUpdateSuccess =
                await _ArticlesService.UpdateArtic(id, Title, PhotoUrl, Status);
            if (isUpdateSuccess)
                return NotFound();
            else
            {
                if (type == 0)
                {
                    bool isUpdatePostSuccess = await _ArticlesService.UpdatePost(id, content, regionId, photoUrlList);
                    if (isUpdatePostSuccess) return NoContent();
                    return NotFound();
                }
                else if (type == 1)
                {
                    //bool isUpdatePostSuccess = await _ArticlesService.UpdatePost(id, content, regionId, photoUrlList);
                    //if (isUpdatePostSuccess) return NoContent();
                    return NotFound();
                }
            }
            //_context.Entry(article).State = EntityState.Modified;
            //return BadRequest();想不出來要放哪
            return NotFound();
        }


    }
}
