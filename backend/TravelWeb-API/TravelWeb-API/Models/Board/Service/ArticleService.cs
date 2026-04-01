using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
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

        public (List<ArticleDataDTO> ArticleDTOList, int TotalCount) GetArticles(int page, string? userId)
        {
            IQueryable<Article> data = _context.Articles.Where(a => a.Status == 1);
            return BuildPagedResult(data, page, userId);
        }

        public (List<ArticleDataDTO> ArticleDTOList, int TotalCount) ArticlesByKeyword(int page, string keyword, string? userId)
        {
            IQueryable<Article> data = _context.Articles
                .Where(a => a.Status == 1)
                .Where(a => (a.Title != null && a.Title.Contains(keyword)) ||
                a.UserId.Contains(keyword));
            return BuildPagedResult(data, page, userId);
        }

        public (List<ArticleDataDTO> ArticleDTOList, int TotalCount) ArticlesByDate(int page, DateTime startTime, DateTime endTime, string? userId)
        {
            // startTime = Convert.ToDateTime("2010/05/07");
            endTime = endTime.AddDays(1);

            IQueryable<Article> data = _context.Articles
                .Where(a => a.Status == 1)
                .Where(a => a.CreatedAt >= startTime &&
                      a.CreatedAt <= endTime);

            return BuildPagedResult(data, page, userId);
        }

        public (List<ArticleDataDTO> ArticleDTOList, int TotalCount) ArticlesByTags(int page, SearchByTagsDTO searchByTags, string? userId)
        {
            IQueryable<Article> data;
            var tagIds = searchByTags.TagsId;

            if (searchByTags.isprecise)
            {
                data = _context.ArticleTags
                      .Where(at => tagIds.Contains(at.TagId))
                      .GroupBy(at => at.ArticleId)
                      .Where(g => g.Select(x => x.TagId).Distinct().Count() == tagIds.Count)
                      .Select(g => g.First().Article);
            }
            else
            {
                data = _context.ArticleTags
                              .Where(t => tagIds.Contains(t.TagId))
                              .Select(t => t.Article)
                              .Where(a => a.Status == 1);

            }

            return BuildPagedResult(data, page, userId);
        }

        public (List<ArticleDataDTO> ArticleDTOList, int TotalCount) ArticlesByAuthorID(int page, string authorID, string? userId)
        {
            IQueryable<Article> data = _context.Articles
                .Where(a => a.Status == 1)
                .Where(a => a.UserId == authorID);

            return BuildPagedResult(data, page, userId);
        }

        public (List<ArticleDataDTO> ArticleDTOList, int TotalCount) Search(int page, ArticleSearchDTO dto, string? userId)
        {
            IQueryable<Article> query = _context.Articles.Where(a => a.Status == 1).AsQueryable();

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

            return BuildPagedResult(query, page, userId);
        }

        private (List<ArticleDataDTO> ArticleDTOList, int TotalCount) BuildPagedResult(
                                                     IQueryable<Article> data, int page, string? userID)
        {
            List<ArticleDataDTO> result = GetArticles(page, data, userID);
            int totalCount = data.Count();
            //GetArticleCount(articles);
            //List<ArticleDataDTO> result = ToArticleDTO(articles,userID);
            return (result, totalCount);
        }

        List<ArticleDataDTO> GetArticles(int page, IQueryable<Article> data, string? userID)
        {
            int pageSize = 10;
            return data
                .OrderByDescending(a => a.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(a => new ArticleDataDTO
                {
                    articleId = a.ArticleId,
                    title = a.Title,
                    CreatedAt = a.CreatedAt,
                    photoUrl = a.PhotoUrl,
                    userID = a.UserId,
                    userName = a.MemberInformation.Name,
                    userAvatar = a.MemberInformation.AvatarUrl,
                    RegionID = a.RegionID,
                    RegionName = a.Region != null
                        ? $"{a.Region.UidNavigation.RegionName},{a.Region.RegionName}"
                        : null,                    
                    LikeCount = a.ArticleLikes.Count(),
                    isLike = a.ArticleLikes.Any(l => l.UserId == userID),
                    tags = a.ArticleTags
                        .Select(t => new TagDTO
                        { TagId = t.TagId, TagName = t.Tag.TagName, icon = t.Tag.icon })
                        .ToList(),
                    CommentCount = a.Comments.Count(),
                    
                })
                .ToList();
        }

        //List<Article> GetArticles(int page, IQueryable<Article> data, string? userID)
        //{
        //    //處理分頁+把資料從DB撈出來
        //    int pageSize = 10;
        //    var result = data
        //        .Where(a => a.Status == 1)
        //        .Distinct() //移除重複資料
        //        .Include(a => a.MemberInformation)
        //        .Include(a => a.ArticleLikes)
        //        .Include(a => a.ArticleTags)
        //        .ThenInclude(t => t.Tag)
        //        .Include(a => a.Region)
        //        .ThenInclude(r => r!.UidNavigation)
        //        .Include(a=>a.Comments)
        //        .OrderByDescending(a => a.CreatedAt)
        //       .Skip((page - 1) * pageSize)
        //       .Take(pageSize)
        //       .ToList();
        //    return result;
        //}

        //int GetArticleCount(List<Article> articles)
        //{
        //    return articles.Count;
        //}


        //List<ArticleDataDTO> ToArticleDTO(List<Article> data, string? userID)
        //{
        //    //資料扁平化
        //    var result = data.Select(a => new ArticleDataDTO
        //    {
        //        articleId = a.ArticleId,
        //        title = a.Title,
        //        CreatedAt = a.CreatedAt,
        //        photoUrl = a.PhotoUrl,
        //        userID = a.UserId,
        //        userName = a.MemberInformation.Name,
        //        userAvatar = a.MemberInformation.AvatarUrl,
        //        RegionID = a.RegionID,
        //        RegionName = ($"{a.Region?.UidNavigation?.RegionName},{a.Region?.RegionName}"),
        //        LikeCount = a.ArticleLikes.Count,
        //        isLike = a.ArticleLikes.Any(l => l.UserId == userID),
        //        tags = a.ArticleTags
        //        .Select(t => new TagDTO { TagId = t.TagId, TagName = t.Tag.TagName }).ToList(),
        //        CommentCount = a.Comments.Count,

        //    }).ToList();

        //    return result;
        //}

        public void Like(int articleID, string userID)
        {
            ArticleLike? like = isLiked(articleID, userID);
            if (like == null)
            {
                _context.ArticleLikes
                    .Add(new ArticleLike { ArticleId = articleID, UserId = userID });
            }
            else
            {
                _context.ArticleLikes.Remove(like);
            }
            _context.SaveChanges();
        }

        ArticleLike? isLiked(int articleID, string userID)
        {
            var like
                = _context.ArticleLikes
                .FirstOrDefault(c => c.ArticleId == articleID && c.UserId == userID);
            return like;
        }

        public (List<ArticleDataDTO> ArticleDTOList, int TotalCount) ArticlesByUserID(int page, string userId)
        {
            int pageSize = 10;
            var data = _context.Articles
                .Where(a => a.UserId == userId);

            return BuildPagedResult(data, page, userId);
        }

        public async Task<bool> DeleteArticle(int articleID, string? authorID)
        {
            var article = await _context.Articles
                .Where(a => a.ArticleId == articleID)
                .Include(a => a.Comments)
                .ThenInclude(c => c.CommentPhotos)
                .Include(a => a.Comments)
                .ThenInclude(c => c.CommentLikes)
                .Include(a => a.Comments)
                .ThenInclude(c => c.InverseParent)
                .ThenInclude(ic => ic.CommentLikes)
                .Include(a => a.Comments)
                .ThenInclude(c => c.InverseParent)
                .ThenInclude(ic => ic.CommentPhotos)
                .Include(a => a.ArticleLikes)
                .Include(a => a.ArticleTags)
                .Include(a => a.ArticleFolders)
                .Include(a => a.Post)
                                
                .FirstOrDefaultAsync(); ;

            if (article != null && article.UserId == authorID)
            {
                _context.Articles.Remove(article);
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
            //.Include(a=>a.Journal)
            //.Include(a=>a.JournalPages)
            //.Include(a=>a.jo)
            //.ThenInclude(j=>j.)            
        }

        public async Task Collect(int articleID, string userId)
        {
            ArticleFolder? collect = isCollect(articleID, userId);
            if (collect == null)
            {
                await _context.ArticleFolders
                    .AddAsync(new ArticleFolder { ArticleId = articleID, UserId = userId });
            }
            else
            {
                _context.ArticleFolders.Remove(collect);
            }
           await _context.SaveChangesAsync();
        }

        ArticleFolder? isCollect(int articleID, string userID)
        {
            var collect
                = _context.ArticleFolders
                .FirstOrDefault(c => c.ArticleId == articleID && c.UserId == userID);
            return collect;
        }

        public (List<ArticleDataDTO> ArticleDTOList, int TotalCount) ArticlesByCollect(int page, string userId)
        {
            var data = _context.ArticleFolders
                .Where(c => c.UserId == userId)
                .Select(c => c.Article);
            return BuildPagedResult(data, page, userId);
        }

        public (List<ArticleDataDTO> ArticleDTOList, int TotalCount) GetArticlesBySource(int page, string productCode)
        {
            throw new NotImplementedException();
        }
    }
    
}
