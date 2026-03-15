using Azure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        //要分頁
        // GET: api/Articles 瀏覽(全部文章之瀑布流)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Article>>> GetArticles()
        {
            return await _context.Articles.ToListAsync();
        }

        //// GET: api/Articles 瀏覽(單一文章詳情)
        //[HttpGet]
        //public async Task<ActionResult<Article>> GetArticleByID(int id)
        //{
        //    Article? article = _ArticlesService.GetArticle(id);
        //    if(article==null)return NotFound();
        //    return article;
        //}


        //// GET:用標題KeyWord搜尋
        //[HttpGet]
        //public async Task<ActionResult<IEnumerable<Article>>> GetArticlesByTitle(string KeyWord)
        //{
        //    return await _context.Articles.ToListAsync();
        //}

        //// GET:用作者搜尋
        //[HttpGet]
        //public async Task<ActionResult<IEnumerable<Article>>> GetArticlesByAuthor(string AuthorID)
        //{
        //    return await _context.Articles.ToListAsync();
        //}

        //// GET:綜合搜尋
        //[HttpGet]
        //public async Task<ActionResult<IEnumerable<Article>>> GetArticlesByAuthor(string AuthorID)
        //{
        //    return await _context.Articles.ToListAsync();
        //}


        // POST: api/Articles 新增標頭
        [HttpPost]
        public async Task<ActionResult<Article>> PostArticle(byte Type, string UserId)
        {
            Article article = _ArticlesService.AddArtic(Type, UserId);
            await _context.SaveChangesAsync();
            return CreatedAtAction(
                   "GetArticle",                       // 1. Action 名稱：指向「查詢單一資料」的那個方法
                   new { id = article.ArticleId },    // 2. 路由參數：用來填補 GetArticle 所需的 id
                   article                            // 3. 回傳內容：要把整份物件秀給前端看
                     );
        }


       
        // DELETE: api/Articles/5 刪除
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteArticle(int id)
        {
                return NotFound();
        }

        [HttpGet("test")]
        public IActionResult TaskArtcle()        
        {
            var list = _context.Articles.ToList();
            var result = 
                list.Select(l => new Test { Title = l.Title, content = l.CreatedAt.ToString()}).ToList();
            return Ok(result);
        }
        
    }
}
