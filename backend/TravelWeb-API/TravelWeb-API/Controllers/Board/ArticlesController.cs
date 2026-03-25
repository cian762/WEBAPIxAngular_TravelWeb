using Azure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using TravelWeb_API.Models.Board.DbSet;
using TravelWeb_API.Models.Board.DTO;
using TravelWeb_API.Models.Board.IService;
using TravelWeb_API.Models.Board.Service;
using TravelWeb_API.Models.MemberSystem;
using static System.Runtime.InteropServices.JavaScript.JSType;

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
        private readonly IArticleService _ArticleService;

        public ArticlesController(BoardDbContext context,
            IPostService noteService,
            IArticleService articleService,
            MemberSystemContext memberDb)
        {
            _context = context;
            _PostService = noteService;
            _ArticleService = articleService;
            _memberDb = memberDb;
        }

        //要分頁
        // GET: api/Articles 瀏覽(全部文章之瀑布流)async Task<ActionResult<IEnumerable<Article>>>
        [HttpGet("Bypage/{page}")]
        public IActionResult GetArticlesByDate(int page)
        {            
            var result = _ArticleService.GetArticles(page);
            return Ok(new
            {
                totalCount = result.TotalCount,
                articleList = result.Item1
            });
        }

        [HttpGet("{id}")]
        public IActionResult GetArticlesByID(int id)
        {
            return Ok(_context.Articles.FirstOrDefault(x => x.ArticleId == id));
        }



        // GET:用標題KeyWord搜尋
        [HttpGet("search")]
        public IActionResult GetArticlesByTitle([FromQuery] int page, [FromQuery] string keyword)
        {
            var result = _ArticleService.ArticlesByKeyword(page, keyword);
            return Ok(new
            {
                totalCount = result.Item2,
                articleList = result.Item1
            });
        }

        [HttpGet("searchByAll")]
        public IActionResult Search([FromQuery] int page, [FromQuery] ArticleSearchDTO dto)
        {
            var result = _ArticleService.Search(page,dto);

            return Ok(new
            {
                totalCount = result.Item2,
                articleList = result.Item1
            });
        }

        //// GET:用日期搜尋
        [HttpGet("searchByDate")]
        public IActionResult GetArticlesByDate(int page, DateTime startTime, DateTime endTime)
        {

            var result = _ArticleService.ArticlesByDate(page, startTime, endTime);
            return Ok(new
            {
                totalCount = result.Item2,
                articleList = result.Item1
            });
        }

        // GET:Tag搜尋
        [HttpGet("searchByTags")]
        public IActionResult GetArticlesByTags([FromQuery]int page, [FromQuery] SearchByTagsDTO searchByTags)
        {
            var result = _ArticleService.ArticlesByTags(page, searchByTags);
            
            return Ok(new
            {
                totalCount = result.Item2,
                articleList = result.Item1
            });
        }

        // GET:用作者搜尋
        [HttpGet("searchByAuthor")]
        public IActionResult GetArticlesByAuthor([FromQuery] int page, [FromQuery]string authorID)
        {
            var result = _ArticleService.ArticlesByAuthorID(page, authorID);

            return Ok(new
            {
                totalCount = result.Item2,
                articleList = result.Item1
            });
        }




        // POST: api/Articles 新增標頭
        [HttpPost]
        public async Task<IActionResult>PostArticle(byte Type)
        {
            // 從 Cookie 取出 Token  
            string? token = Request.Cookies["AuthToken"];
            string? userId = GetUser.Id(token);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("無效的 Token");
            Article article = _PostService.AddArtic(Type, userId);
            await _context.SaveChangesAsync();
            return Ok(article.ArticleId);
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
                list.Select(l => new Test { Title = l.Title, PhotoUrl = l.PhotoUrl, UserName = l.UserId }).ToList();
            return Ok(result);
        }


    }
}
