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
            // 從 Cookie 取出 Token  
            string? token = Request.Cookies["AuthToken"];
            string? userId = GetUser.Id(token);

            var result = _ArticleService.GetArticles(page, userId);
            return Ok(new
            {
                totalCount = result.TotalCount,
                articleList = result.ArticleDTOList
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
            // 從 Cookie 取出 Token  
            string? token = Request.Cookies["AuthToken"];
            string? userId = GetUser.Id(token);
           
            var result = _ArticleService.ArticlesByKeyword(page, keyword, userId);
            return Ok(new
            {
                totalCount = result.TotalCount,
                articleList = result.ArticleDTOList
            });
        }

        [HttpGet("searchByAll")]
        public IActionResult Search([FromQuery] int page, [FromQuery] ArticleSearchDTO dto)
        {
            // 從 Cookie 取出 Token  
            string? token = Request.Cookies["AuthToken"];
            string? userId = GetUser.Id(token);            
            var result = _ArticleService.Search(page,dto, userId);

            return Ok(new
            {
                totalCount = result.TotalCount,
                articleList = result.ArticleDTOList
            });
        }

        //// GET:用日期搜尋
        [HttpGet("searchByDate")]
        public IActionResult GetArticlesByDate(int page, DateTime startTime, DateTime endTime)
        {
            // 從 Cookie 取出 Token  
            string? token = Request.Cookies["AuthToken"];
            string? userId = GetUser.Id(token);            

            var result = _ArticleService.ArticlesByDate(page, startTime, endTime, userId);
            return Ok(new
            {
                totalCount = result.TotalCount,
                articleList = result.ArticleDTOList
            });
        }

        // GET:Tag搜尋
        [HttpGet("searchByTags")]
        public IActionResult GetArticlesByTags([FromQuery]int page, [FromQuery] SearchByTagsDTO searchByTags)
        {
            // 從 Cookie 取出 Token  
            string? token = Request.Cookies["AuthToken"];
            string? userId = GetUser.Id(token);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("無效的 Token");
            var result = _ArticleService.ArticlesByTags(page, searchByTags, userId);
            return Ok(new
            {
                totalCount = result.TotalCount,
                articleList = result.ArticleDTOList
            });
        }

        // GET:用作者搜尋
        [HttpGet("searchByAuthor")]
        public IActionResult GetArticlesByAuthor([FromQuery] int page, [FromQuery]string authorID)
        {
            // 從 Cookie 取出 Token  
            string? token = Request.Cookies["AuthToken"];
            string? userId = GetUser.Id(token);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("無效的 Token");
            var result = _ArticleService.ArticlesByAuthorID(page, authorID, userId);
            return Ok(new
            {
                totalCount = result.TotalCount,
                articleList = result.ArticleDTOList
            });
        }

        //GET:用戶主頁
       [HttpGet("articlesByUser")]
        public IActionResult GetArticlesByUser([FromQuery]int page)
        {
            // 從 Cookie 取出 Token  
            string? token = Request.Cookies["AuthToken"];
            string? userId = GetUser.Id(token);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("無效的 Token");
            var result = _ArticleService.ArticlesByUserID(page, userId);
            return Ok(new
            {
                totalCount = result.TotalCount,
                articleList = result.ArticleDTOList
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


        //// POST: 收藏
        [HttpPost("Like")]
        public async Task<IActionResult> ArticleLike(int articleID)
        {
            // 從 Cookie 取出 Token  
            string? token = Request.Cookies["AuthToken"];
            string? userId = GetUser.Id(token);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("無效的 Token");
            _ArticleService.Like(articleID,userId);
            return Ok();
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
