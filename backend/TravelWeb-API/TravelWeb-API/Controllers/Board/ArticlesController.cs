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
        private readonly IPostService _PostService;
        
        public ArticlesController(BoardDbContext context,
            IPostService noteService,
            MemberSystemContext memberDb)
        {
            _context = context;
            _PostService = noteService;
            _memberDb = memberDb;
        }

        //要分頁
        // GET: api/Articles 瀏覽(全部文章之瀑布流)async Task<ActionResult<IEnumerable<Article>>>
        [HttpGet]
        public IActionResult GetArticles()
        { 
            return Ok(_context.Articles.Select(a=>new { a.Title }).ToList());
        }

        [HttpGet("{id}")]
        public IActionResult GetArticlesByID(int id)
        {
            return Ok(_context.Articles.FirstOrDefault(x=>x.ArticleId == id));
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
        public async Task<ActionResult<int>> PostArticle(byte Type, string UserId)
        {
            Article article = _PostService.AddArtic(Type, UserId);
            await _context.SaveChangesAsync();
            return article.ArticleId;
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
                list.Select(l => new Test {ArticleId=l.ArticleId, Title = l.Title,PhotoUrl=l.PhotoUrl,UserName =l.UserId}).ToList();
            return Ok(result);
        }
        
    }
}
