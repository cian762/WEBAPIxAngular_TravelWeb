using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using TravelWeb_API.Models.Board.DbSet;
using TravelWeb_API.Models.Board.DTO;
using TravelWeb_API.Models.Board.IService;
using TravelWeb_API.Models.MemberSystem;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace TravelWeb_API.Models.Board.Service
{
    public class ArticleService : IArticleService
    {
        private readonly BoardDbContext _context;
        private readonly MemberSystemContext _memberDb;
        public ArticleService(BoardDbContext context, MemberSystemContext memberDb)
        {
            _context = context;
            _memberDb = memberDb;
        }

        public (List<ArticleDataDTO>, int TotalCount) GetArticles(int page)
        {
            var data = _context.Articles.Include(a => a.MemberInformation).ToList();
            var totalCount = data.Count;
            var result = resultDTO(page, data);
            return (result, totalCount);
        }

        public (List<ArticleDataDTO>, int TotalCount) ArticlesByKeyword(int page,string keyword)
        {            
            var data = _context.Articles
                .Where(
                a => (a.Title != null && a.Title.Contains(keyword)) ||
                a.UserId.Contains(keyword)
                ).Include(a => a.MemberInformation).ToList();

            var totalCount = data.Count;
            return (resultDTO(page, data), totalCount);
        }

        public (List<ArticleDataDTO>, int TotalCount) ArticlesByDate(int page, DateTime startTime,DateTime endTime)
        {            
           // startTime = Convert.ToDateTime("2010/05/07");
            endTime = endTime.AddDays(1);

            var data = _context.Articles
                .Where(a => a.CreatedAt >= startTime && 
                      a.CreatedAt <= endTime).Include(a => a.MemberInformation).ToList();

            var totalCount = data.Count;
            return (resultDTO(page, data), totalCount);
        }

        public (List<ArticleDataDTO>, int TotalCount) ArticlesByTags(int page, SearchByTagsDTO searchByTags)
        {
            int pageSize = 10;
            List<Article> data = new List<Article>();
            var tagIds = searchByTags.TagsId;
            
            if (searchByTags.isprecise)
            {
                data = _context.ArticleTags
                      .Where(at => tagIds.Contains(at.TagId))
                      .GroupBy(at => at.ArticleId)
                      .Where(g => g.Select(x => x.TagId).Distinct().Count() == tagIds.Count)
                      .Select(g => g.First().Article)
                      .Include(a => a.MemberInformation)
                      .ToList();
            }
            else
            {
                data = _context.ArticleTags
                              .Where(t => tagIds.Contains(t.TagId))
                              .Select(t => t.Article)
                              .Distinct() //移除重複資料
                              .Include(a => a.MemberInformation)
                              .ToList();
            }

            
            var totalCount = data.Count;
            return (resultDTO(page, data), totalCount);

        }

        public (List<ArticleDataDTO>, int) ArticlesByAuthorID(int page, string authorID)
        {            
            var data = _context.Articles
                .Where(a => a.UserId == authorID)
                .Include(a => a.MemberInformation)
                .ToList();
            var totalCount = data.Count;
            return (resultDTO(page,data), totalCount);
        }

        public (List<ArticleDataDTO>, int) Search(int page, ArticleSearchDTO dto)
        {
            var query = _context.Articles.AsQueryable();

            // 標題
            if (!string.IsNullOrEmpty(dto.Keyword))
            {
                query = query.Where(a => a.Title.Contains(dto.Keyword));
            }

            // 日期
            if (dto.StartTime.HasValue)
            {
                query = query.Where(a => a.CreatedAt >= dto.StartTime.Value);
            }

            if (dto.EndTime.HasValue)
            {
                query = query.Where(a => a.CreatedAt <= dto.EndTime.Value);
            }

            // 作者
            if (!string.IsNullOrEmpty(dto.AuthorId))
            {
                query = query.Where(a => a.UserId == dto.AuthorId);
            }

            // Tags（全部符合）
            if (dto.TagIds != null && dto.TagIds.Any())
            {
                var tagIds = dto.TagIds;
                query = _context.ArticleTags
                               .Where(t => tagIds.Contains(t.TagId))
                               .Select(t => t.Article)
                               .Distinct();
            }

            var totalCount = query.Count();
            var data = query.Include(a => a.MemberInformation).ToList();
            return (resultDTO(page, data), totalCount);
        }


        List<ArticleDataDTO> resultDTO(int page, List<Article> data)
        {
            //處理分頁
            int pageSize = 10;
            var result = data.OrderByDescending(a => a.CreatedAt)
               .Skip((page - 1) * pageSize)
               .Take(10)
               .ToList();

            //資料扁平化
            var r = result.Select(a => new ArticleDataDTO
            {
                articleId = a.ArticleId,
                title = a.Title,
                CreatedAt = a.CreatedAt,
                photoUrl = a.PhotoUrl,
                userID = a.UserId,
                userName = a.MemberInformation.Name,
                userAvatar = a.MemberInformation.AvatarUrl
            }).ToList();

            return r;
        }
    }
}
