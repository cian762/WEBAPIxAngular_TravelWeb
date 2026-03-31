using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using System.Security.Claims;
using System.Threading.Tasks;
using TravelWeb_API.Models.Board.DbSet;
using TravelWeb_API.Models.Board.DTO;
using TravelWeb_API.Models.Board.IService;
using TravelWeb_API.Models.MemberSystem;

namespace TravelWeb_API.Models.Board.Service
{
    public class PostService : IPostService
    {
        private readonly BoardDbContext _context;
        private readonly MemberSystemContext _memberDb;
        public PostService(BoardDbContext context, MemberSystemContext memberDb)
        {
            _context = context;
            _memberDb=memberDb;
        }

        public Article AddArtic(byte Type,string UserId)
        {
            Article article = new Article();
            article.Type = Type;
            article.CreatedAt = DateTime.Now;
            article.Status = 0;                        
            article.IsViolation = false;
            article.UserId = UserId;
            _context.Articles.Add(article);
            SaveChange();
            return article;
        }


        public async Task<bool> UpdateArtic(int id,string? Title,string? PhotoUrl,byte Status,int? regionId)
        {
            Article? article = 
            _context.Articles.FirstOrDefault(a => a.ArticleId == id);
            if (article == null) return false;
            else
            {
                article.Title = Title;
                article.PhotoUrl = PhotoUrl;
                article.Status = Status;                
                article.UpdatedAt = DateTime.Now;
                article.RegionID = regionId;
                await _context.SaveChangesAsync();
                return true;
            }

            
        }
        //article.IsViolation

        public void AddPost(Article article)
        {
            var post = new Post
            {
                // 不要設定 ArticleId = article.ArticleId; 
                Article = article // 直接把物件塞給導覽屬性
            };
            _context.Posts.Add(post);
            SaveChange();
        }
        public void SaveChange()
        {
           _context.SaveChanges();
        }

        public Article? GetArticle(int id)
        {          
            Article? article = _context.Articles
                //.Include(a => a.MemberCode)
                .FirstOrDefault(a => a.ArticleId == id);

            // 判斷邏輯集中在這裡|| article.MemberCode == null
            if (article == null) return null;

            return article;
        }



        public async Task<PostDetailDto?> GetPostDetailed(int id, string? currentUserId)
        {
            var postDetail = await _context.Articles.Where(a => a.ArticleId == id)
                .Select(
                a => new PostDetailDto
                {
                    Type = a.Type,
                    Title = a.Title,
                    CreatedAt = a.CreatedAt,
                    UpdatedAt = a.UpdatedAt,
                    Cover = a.PhotoUrl,
                    Contents = a.Post!.Contents,
                    RegionId = a.RegionID,
                    RegionName = a.Region != null ? a.Region.RegionName : null,
                    PostPhoto = a.PostPhotos
                .Where(p => p.ArticleId == a.ArticleId)
                .Select(p => p.Photo).ToList(),
                    AuthorID = a.UserId,
                    AuthorName = a.MemberInformation.Name,
                    AvatarUrl = a.MemberInformation.AvatarUrl,
                    Status = a.Status,
                    CommentCount= a.Comments.Count,
                    LikeCount=a.ArticleLikes.Count,
                    isLike=a.ArticleLikes.Any(l=>l.UserId==currentUserId)
                }).FirstOrDefaultAsync();          

            return postDetail;
        }

        

        public Journal GetJournalDetailed(int id)
        {
            throw new NotImplementedException();
        }


        public async Task<bool> UpdatePost(int id, string? content, int? regionId
            ,List<string>? photos)
        {
            
            Post? post =
                _context.Posts.FirstOrDefault(a => a.ArticleId == id);
            if (GetArticleByID(id) == null || post == null) return false;
            else
            {
                post.Contents = content;
                post.RegionId = regionId;


                var postPhotos = _context.PostPhotos.Where(p => p.ArticleId == id);
                foreach (var p in postPhotos)
                {
                    _context.PostPhotos.Remove(p);
                }
                foreach (var p in photos)
                {
                    _context.PostPhotos.Add(new PostPhoto { ArticleId = id, Photo = p });
                }
                await _context.SaveChangesAsync();
                return true;
            }
        }
        Article? GetArticleByID(int id)
        {
            return _context.Articles.FirstOrDefault(a => a.ArticleId == id);
        }

        Post? GetPostByID(int id)
        {
            return _context.Posts.FirstOrDefault(e => e.ArticleId == id);
        }

        MemberInformation Author(string CommentAuthorID)
        {
            MemberInformation? member = 
            _memberDb.MemberInformations.FirstOrDefault(a => a.MemberId == CommentAuthorID);
            return member;
        }
              

        

       
    }
}
