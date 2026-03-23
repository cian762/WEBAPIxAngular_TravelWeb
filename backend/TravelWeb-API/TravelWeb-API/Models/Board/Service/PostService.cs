using Microsoft.EntityFrameworkCore;
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


        public async Task<bool> UpdateArtic(int id,string? Title,string? PhotoUrl,byte Status)
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
        
        

        public PostDetailDto? GetPostDetailed(Article article)
        {           
            if(article.Type == 0 && article.Post != null)
                {
                Post post = article.Post;
                MemberInformation? author = 
                    _memberDb.MemberInformations.FirstOrDefault(x=>x.MemberId==article.UserId);
                PostDetailDto postDetail = new PostDetailDto();
                postDetail.Type = article.Type;
                postDetail.Title = article.Title;
                postDetail.CreatedAt = article.CreatedAt;
                postDetail.UpdatedAt = article.UpdatedAt;
                postDetail.Cover = article.PhotoUrl;
                postDetail.Contents = post.Contents;
                postDetail.RegionId = post.RegionId;
                postDetail.PostPhoto = _context.PostPhotos
                    .Where(p => p.ArticleId == article.ArticleId)
                    .Select(p => p.Photo).ToList();
                postDetail.AuthorName = author.Name;
                postDetail.AvatarUrl = author.AvatarUrl;
                postDetail.Status = article.Status; 
                return postDetail;
            }

            else if (article.Type == 1 && article.Journal != null)
            {
                return null;
            }
            else
            {
                return null;
            }
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
