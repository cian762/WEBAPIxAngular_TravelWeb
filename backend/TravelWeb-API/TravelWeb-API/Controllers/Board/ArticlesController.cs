using Azure;
using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using TravelWeb_API.Models.ActivityModel;
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
        private readonly ActivityDbContext _activityDB;
        private readonly IPostService _PostService;
        private readonly IArticleService _ArticleService;


        public ArticlesController(BoardDbContext context,
            IPostService noteService,
            IArticleService articleService,
            MemberSystemContext memberDb, ActivityDbContext activityDB)
        {
            _context = context;
            _PostService = noteService;
            _ArticleService = articleService;
            _memberDb = memberDb;
            _activityDB = activityDB;
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

        //熱門文章
        [HttpGet("trending")]
        public async Task<ActionResult<List<Trending>>> GetTrendings()
        {
            var result = await _ArticleService.GetTrendings();
            return Ok(result);
        }

        [HttpGet("{id}")]
        public IActionResult GetArticlesByID(int id)
        {
            return Ok(_context.Articles.FirstOrDefault(x => x.ArticleId == id));
        }

        //熱門文章(訪客版)
        [HttpGet("Visitors")]
        public async Task<ActionResult<List<ArticleDataDTO>>> GetTrendingsForVisitors()
        {
            var result = _ArticleService.GetTrendingsForVisitors();
            return Ok(result);
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
            string? currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(currentUserId))
                return Unauthorized("無效的 Token");
            var result = _ArticleService.ArticlesByTags(page, searchByTags, currentUserId);
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
            string? currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(currentUserId))
                return Unauthorized("無效的 Token"); 

            var result = _ArticleService.ArticlesByAuthorID(page, authorID, currentUserId);
            return Ok(new
            {
                
                totalCount = result.TotalCount,
                articleList = result.ArticleDTOList
            });
        }

        [HttpGet("searchBySource")]
        public IActionResult GetArticlesBySource([FromQuery] int page, [FromQuery] string productCode)
        {
            // 從 Cookie 取出 Token  
            string? currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(currentUserId))
                return Unauthorized("無效的 Token");
            var result = _ArticleService.GetArticlesBySource(page, productCode);
            return Ok(new
            {
                totalCount = result.TotalCount,
                articleList = result.ArticleDTOList
            });
        }

        //GET:用戶主頁:自己發布的私人文章
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

        //GET:用戶收藏的文章
        [HttpGet("articlesByCollect")]
        public IActionResult GetArticlesByCollect([FromQuery] int page)
        {
            // 從 Cookie 取出 Token  
            string? currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = _ArticleService.ArticlesByCollect(page, currentUserId);
            return Ok(new
            {
                totalCount = result.TotalCount,
                articleList = result.ArticleDTOList
            });
        }

        [HttpGet("articlesByFollowed")]
        public IActionResult GetArticlesByFollowed([FromQuery] int page)
        {
            // 從 Cookie 取出 Token  
            string? currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(currentUserId)) return NotFound();
            var result = _ArticleService.ArticlesByFollowed(page, currentUserId);
            return Ok(new
            {
                totalCount = result.TotalCount,
                articleList = result.ArticleDTOList
            });

        }

        [HttpGet("curUser")]
        public IActionResult GetCurUser([FromQuery] int page)
        {
            // 從 Cookie 取出 Token  
            string? token = Request.Cookies["AuthToken"];
            string? userId = GetUser.Id(token);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("無效的 Token");
            AuthorInfo? member = _memberDb.MemberInformations
                .Where(m=>m.MemberId==userId)
                .Select(m=>new AuthorInfo
                {
                    authorId = m.MemberId,
                    authorName = m.Name,
                    avatarUrl = m.AvatarUrl,                    
                    isCurrentUser = true,   
                })
                .FirstOrDefault();

            member.ArticleCount = _context.Articles.Where(a => a.UserId == userId).Count();
            return Ok(member);
        }

        [HttpGet("authorUser")]
        public async Task <ActionResult> GetAuthorUser([FromQuery]string authorID)
        {
            string? token = Request.Cookies["AuthToken"];
            string? currentUserId = GetUser.Id(token);
            if (string.IsNullOrEmpty(currentUserId))
                return Unauthorized("無效的 Token");
            bool IsSelf = authorID == currentUserId;
            if (IsSelf) return Ok();

            var member = _memberDb.MemberInformations
                .FirstOrDefault(m => m.MemberId == authorID);
            if (member==null) return NotFound(new { message = "找不到該名會員或您無權查看此頁面" });
            if (!string.IsNullOrEmpty(currentUserId))
            {
                var myInfo = await _context.MemberInformations.FirstOrDefaultAsync(m => m.MemberId == currentUserId);
                if (myInfo != null)
                {                    

                    bool isBlockedByMe = await _memberDb.Blockeds
                        .AnyAsync(b => b.MemberId == currentUserId && b.BlockedId == authorID); // 我封鎖他

                    bool isBlockingMe = await _memberDb.Blockeds
                        .AnyAsync(b => b.MemberId == authorID && b.BlockedId == currentUserId); // 他封鎖我

                    if (isBlockedByMe || isBlockingMe)
                    {
                        return NotFound(new { message = "找不到該名會員或您無權查看此頁面" });
                    }
                }

            }
            return Ok(member);
        }

        [HttpGet("getAuthorUserInfo")]
        public async Task<ActionResult<AuthorInfo>> GetAuthorUserInfo([FromQuery] string authorID)
        {
            // 從 Cookie 取出 Token  
            string? currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(currentUserId)) return NotFound();
            AuthorInfo? author = await _memberDb.MemberInformations
                .Where(m => m.MemberId == authorID)
                .Select(m => new AuthorInfo
                {
                    authorName = m.Name,
                    avatarUrl = m.AvatarUrl,
                    isCurrentUser = (currentUserId == m.MemberId),
                    ArticleCount = 0

                }).FirstOrDefaultAsync();
            if (author == null) return NotFound();
            author.ArticleCount = _context.Articles
                .Where(a => a.UserId == authorID && a.Status == 1)
                .ToList().Count();


            return author;

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


        //// POST:點讚
        [HttpPost("collect")]
        public async Task<IActionResult> ArticleCollect(int articleID)
        {
            // 從 Cookie 取出 Token  
            string? currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (currentUserId != null) { 
                await _ArticleService.Collect(articleID, currentUserId);
            }
            

            return Ok();
        }

        //// POST:點讚
        [HttpPost("Like")]
        public async Task<IActionResult> ArticleLike(int articleID)
        {
            // 從 Cookie 取出 Token  
            string? token = Request.Cookies["AuthToken"];
            string? userId = GetUser.Id(token);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("無效的 Token");
            _ArticleService.Like(articleID, userId);
            return Ok();
        }



        // DELETE: api/Articles/5 刪除
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteArticle(int id)
        {
            string? currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            bool isSuccess = await _ArticleService.DeleteArticle(id, currentUserId);
            if (isSuccess)
            {
                return NoContent();
            }
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

        [HttpGet("AllRegions")]
        public async Task<IActionResult> GetAllRegions()
        {
            var regions = await _activityDB.TagsRegions
            .Where(c => c.UidNavigation != null && c.UidNavigation.Uid == null)
            .Select(c => new {  c.RegionId, 
                                c.RegionName,
                                Dist= _activityDB.TagsRegions
                                      .Where(d => d.Uid == c.RegionId)
                                      .Select(d => new { d.RegionId,
                                                         d.RegionName}).ToList()
                                                     }).ToListAsync();
            return Ok(regions);
        }
    }
}
