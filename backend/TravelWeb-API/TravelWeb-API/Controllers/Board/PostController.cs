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
        private readonly IPostService _PostService;

        public PostController(BoardDbContext context,
            IPostService noteService,
            MemberSystemContext memberDb)
        {
            _context = context;
            _PostService = noteService;
            _memberDb = memberDb;
        }

        // GET: api/PostsDetailed 瀏覽Post
        [HttpGet("{id}")]
        public async Task<ActionResult<PostDetailDto>> GetPostDetail(int id)
        {
            Article? article = _context.Articles.Include(a => a.Post).FirstOrDefault(x => x.ArticleId == id);
            if (article == null) return NotFound();
            PostDetailDto postDetail =
                _PostService.GetPostDetailed(article);
            if (postDetail == null) return NotFound();
            return postDetail;
        }

        [HttpPost]
        public async Task<ActionResult<int>> PostPost(int id)
        {
            var article=_context.Articles.FirstOrDefault(a => a.ArticleId == id);
            if (article.Type == 0)
            {
                _PostService.AddPost(article);
                return article.ArticleId;
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
        public async Task<ActionResult<bool>> PutArticle(int id, [FromBody] PostUpdateDto updateDto)
        {

            //修改快速串文
            byte type = _context.Articles.FirstOrDefault(a => a.ArticleId == updateDto.id).Type;
            bool isUpdateSuccess =
                await _PostService.UpdateArtic(
                    updateDto.id,
                    updateDto.Title, 
                    updateDto.PhotoUrl, 
                    updateDto.Status);
            if (!isUpdateSuccess)
                return NotFound();
            else
            {
                if (type == 0)
                {
                    bool isUpdatePostSuccess = await _PostService.UpdatePost(
                        updateDto.id,
                        updateDto.content, 
                        updateDto.regionId,
                        updateDto.photoUrlList);//, photoUrlList
                    if (isUpdatePostSuccess) return true;
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
