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
using TravelWeb_API.Models.Board.Service;
using TravelWeb_API.Models.MemberSystem;

namespace TravelWeb_API.Controllers.Board
{
    [Route("api/Board/[controller]")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "Board")]
    public class ArticlesController : ControllerBase
    {
        private readonly BoardDbContext _context;
        private readonly MemberSystemContext _memberDb;
        private readonly IArticlesService _ArticlesService;
       
        public ArticlesController(BoardDbContext context,
            IArticlesService noteService,
            MemberSystemContext memberDb)
        {
            _context = context;
            _ArticlesService = noteService;
            _memberDb = memberDb;
        }

        // GET: api/Articles 瀏覽
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Article>>> GetArticles()
        {
            return await _context.Articles.ToListAsync();
        }

        // GET: api/PostsDetailed/5
        [HttpGet("{id}")]
        public async Task<ActionResult<PostDetailDto>> GetArticle(int id)
        {            
            Article? article=_ArticlesService.GetArtic(id);
            if (article == null)
            {
                return NotFound();
            }
            else 
            {
                if (article.ArticleId == 0)
                {
                    PostDetailDto postDetail = _ArticlesService.GetPostDetailed(article);
                    return postDetail;
                }
                else if (article.ArticleId == 1)
                {
                    return NotFound();
                }
                else
                {
                    return NotFound();
                }
            }
        }
        




        // POST: api/Articles 新增標頭
        [HttpPost]
        public async Task<ActionResult<Article>> PostArticle(byte Type,string UserId)
        {

            // 1. 建立文章，此時 article.ArticleId 是 0
            Article article = _ArticlesService.AddArtic(Type, UserId);

            if (Type == 0)
            {
                //Post裡有article屬性
                _ArticlesService.AddPost(article);
            }
            else if (Type == 1)
            {

            }
            else
            {
                return BadRequest();
            }
            

            // 3. 執行 SaveChangesAsync
            // EF 非常聰明，它會：
            // a. 先新增 Article，拿到資料庫生成的 ID。
            // b. 自動把這個 ID 填入 Post 的外鍵欄位。
            // c. 接著新增 Post。
            await _context.SaveChangesAsync();
            return CreatedAtAction("GetArticle", new { id = article.ArticleId }, article);
        }

        

        // PUT: api/Articles/5 修改標頭
        [HttpPut("{id}")]
        public async Task<IActionResult> PutArticle(int id, string Title, string PhotoUrl, byte Status)
        {
            //修改快速串文
            bool isUpdateSuccess = await _ArticlesService.UpdateArtic(id, Title, PhotoUrl, Status);
            if (!isUpdateSuccess)
            {
                return NotFound();
            }

            //_context.Entry(article).State = EntityState.Modified;
            //return BadRequest();想不出來要放哪
            return NoContent();
        }

        
        
       
        // DELETE: api/Articles/5 刪除
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteArticle(int id)
        {
            var article = await _context.Articles.FindAsync(id);
            if (article == null)
            {
                return NotFound();
            }

            _context.Articles.Remove(article);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        
    }
}
