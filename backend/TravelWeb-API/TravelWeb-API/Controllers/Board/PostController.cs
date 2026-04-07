using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using TravelWeb_API.Models.ActivityModel;
using TravelWeb_API.Models.Board.DbSet;
using TravelWeb_API.Models.Board.DTO;
using TravelWeb_API.Models.Board.IService;
using TravelWeb_API.Models.Itinerary.Service;
using TravelWeb_API.Models.MemberSystem;

namespace TravelWeb_API.Controllers.Board
{
    [Route("api/[controller]")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "Board")]
    public class PostController : ControllerBase
    {
        private readonly BoardDbContext _context;       
        
        private readonly IPostService _PostService;

        public PostController(BoardDbContext context,
            IPostService noteService)
        {
            _context = context;
            _PostService = noteService;            
        }

        // GET: api/PostsDetailed 瀏覽Post
        [HttpGet("{id}")]
        public async Task<ActionResult<PostDetailDto>> GetPostDetail(int id)
        {   
            string? currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (currentUserId == null) return Unauthorized();
            var post = await _PostService.GetPostDetailed(id, currentUserId);
            if (post == null)
                return NotFound("文章不存在");            
            if (post.Status != 1 && post.AuthorID != currentUserId)
                return NotFound("沒有瀏覽此篇文章的權限"); ;

            return Ok(post);
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
                    updateDto.Status,
                    updateDto.regionId);
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
