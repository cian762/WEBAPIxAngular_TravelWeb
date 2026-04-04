using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using TravelWeb_API.Controllers.Board;
using TravelWeb_API.Models.ActivityModel;
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
        private readonly ActivityDbContext _activityDb;
        public ArticleService(BoardDbContext context, MemberSystemContext memberDb, ActivityDbContext activityDb)
        {
            _context = context;
            _memberDb = memberDb;
            _activityDb = activityDb;
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
                .Where(a => a.Title != null && a.Title.Contains(keyword));
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
            IQueryable<Article> query = _context.Articles.Where(a => a.Status == 1)
                .Include(a => a.MemberInformation)
                .AsQueryable();

            // 標題
            if (!string.IsNullOrEmpty(dto.Keyword))
            {
                query = query.Where(a => a.Title.Contains(dto.Keyword));
            }

            // 日期
            if (!string.IsNullOrEmpty(dto.StartTime))
            {
                var start = DateTime.Parse(dto.StartTime);
                query = query.Where(a => a.CreatedAt >= start);
            }
            if (!string.IsNullOrEmpty(dto.EndTime))
            {
                var end = DateTime.Parse(dto.EndTime).AddDays(1).Date;
                query = query.Where(a => a.CreatedAt < end);
            }

            // 作者
            if (!string.IsNullOrEmpty(dto.authorKeyword))
            {
                query = query
                .Where(a => a.UserId.Contains(dto.authorKeyword) ||
                a.MemberInformation.Name.Contains(dto.authorKeyword));
            }

            //地點
            if (dto.RegionId.HasValue)
            {
                var childIds = _activityDb.TagsRegions
                        .Where(r => r.Uid == dto.RegionId.Value)//上層等於dto.RegionId
                        .Select(r => r.RegionId)
                        .ToList();

                childIds.Add(dto.RegionId.Value);

                query = query.Where(a => childIds.Contains(a.RegionID.Value));
            }

            //// Tags（全部符合）
            //if (dto.TagIds != null && dto.TagIds.Any())
            //{
            //    var tagIds = dto.TagIds;
            //    query = _context.ArticleTags
            //                   .Where(t => tagIds.Contains(t.TagId))
            //                   .Select(t => t.Article)
            //                   .Distinct();
            //}

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
            var blockedIds = _memberDb.Blockeds
                         .Where(b => b.MemberId == userID || b.BlockedId == userID)
                         .Select(b => b.MemberId == userID ? b.BlockedId : b.MemberId)
                         .ToList();
            int pageSize = 10;
            return data
                .OrderByDescending(a => a.CreatedAt)
                .Where(a => !blockedIds.Contains(a.MemberInformation.MemberId))
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(a => new ArticleDataDTO
                {
                    articleId = a.ArticleId,
                    Type = a.Type,
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
                .Where(a => a.UserId == userId && a.Status == 0);

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

        public async Task<List<Trending>> GetTrendings()
        {
            var trending = await _context.Articles
                .Where(a => a.Status == 1).Include(a => a.MemberInformation)
                .Select(a => new
                {
                    Article = a,
                    LikeCount = _context.ArticleLikes.Count(l => l.ArticleId == a.ArticleId),
                    CommentCount = _context.Comments.Count(c => c.ArticleId == a.ArticleId),
                    ViewCount = _context.UserActivityLogs.Count(l => l.TargetId == a.ArticleId),
                    FolderCount = _context.ArticleFolders.Count(f => f.ArticleId == a.ArticleId),
                    HoursSince = EF.Functions.DateDiffHour(a.CreatedAt, DateTime.UtcNow)
                })
                .ToListAsync();

            var TOP = trending
                .Select(x => new
                {
                    x.Article,
                    Score = (x.ViewCount * 1.0 + x.LikeCount * 3.0 + x.CommentCount * 5.0 + x.FolderCount * 4.0)
                            / Math.Pow(x.HoursSince + 2, 1.5)
                })
                .OrderByDescending(x => x.Score)
                .ThenByDescending(x => x.Article.CreatedAt)
                .Take(5)
                .ToList();
            var result = TOP.Select(t =>
            new Trending
            {
                articleId = t.Article.ArticleId,
                title = t.Article.Title,
                Type = t.Article.Type,
                photoUrl = t.Article.PhotoUrl,
                author = t.Article.MemberInformation.Name,

            }
            ).ToList();

            return result;
        }
        

    }
    
}
