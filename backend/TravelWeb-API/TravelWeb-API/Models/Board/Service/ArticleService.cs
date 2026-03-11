using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using TravelWeb_API.Models.Board.DTO;
using TravelWeb_API.Models.Board.IService;
using TravelWeb_API.Models.MemberSystem;

namespace TravelWeb_API.Models.Board.Service
{
    public class ArticleService : IArticlesService
    {
        private readonly BoardDbContext _context;
        private readonly MemberSystemContext _memberDb;
        public ArticleService(BoardDbContext context)
        {
            _context = context;
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


        public async Task<bool> UpdateArtic(int id,string Title,string PhotoUrl,byte Status)
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

        public Article? GetArtic(int id)
        {
            Article? article =
            _context.Articles.FirstOrDefault(a => a.ArticleId == id);

            return article;
        }
        public PostDetailDto GetPostDetailed(Article article)
        {
            int id = article.ArticleId;
            Post? post =
                _context.Posts.FirstOrDefault(p => p.ArticleId == id);
            MemberInformation? Author =
                _memberDb.MemberInformations.FirstOrDefault(a => a.MemberId == article.UserId);
            PostDetailDto postDetail = new PostDetailDto();
            postDetail.Type = article.Type;
            postDetail.Title = article.Title;
            postDetail.CreatedAt = article.CreatedAt;
            postDetail.UpdatedAt = article.UpdatedAt;
            postDetail.Cover = article.PhotoUrl;
            postDetail.Contents = post.Contents;
            postDetail.RegionId = post.RegionId;
            postDetail.PostPhoto =_context.PostPhotos.Where(p => p.ArticleId == id)
                .Select(p=>p.Photo).ToList();
            postDetail.AuthorName = Author.Name;
            postDetail.AvatarUrl = Author.AvatarUrl;
            
            return postDetail;
        }

        public Journal GetJournalDetailed(int id)
        {
            throw new NotImplementedException();
        }


        public void UpdatePost(int id)
        {
            throw new NotImplementedException();
        }

        

        public List<CommentsDTO> GetComments(int id)
        {
            if (article(id) == null) return null;
            
            var CList = _context.Comments.Where(c => c.ArticleId == id)
                .Select(c => new CommentsDTO
                {
                    AuthorName = Author(c.UserId).Name,
                    AvatarUrl = Author(c.UserId).AvatarUrl,
                    Contents = c.Contents,
                    CreatedAt = c.CreatedAt,
                    LikeCount = LikeCount(c),
                    isLiked = false,
                });
            return CList.ToList();
        }

        int LikeCount(Comment comment)
        {
            int id = comment.CommentId;
            int Like = _context.CommentLikes.Where(c => c.CommentId == id).Count();
            return Like;
        }
        Article? article(int id)
        {
            return _context.Articles.FirstOrDefault(a => a.ArticleId == id);
        }

        MemberInformation Author(string CommentAuthorID)
        {
            MemberInformation? member = 
            _memberDb.MemberInformations.FirstOrDefault(a => a.MemberId == CommentAuthorID);
            return member;
        }
              

        public Comment AddComment(int articleID, string UserId, string contents)
        {
            Comment comment = new Comment();
            comment.UserId = UserId;
            comment.ParentId = null;
            comment.ArticleId = articleID;
            comment.Contents = contents;
            //comment.CreatedAt = DateTime.Now;

            _context.Comments.Add(comment);
            _context.SaveChanges();
            return comment;
        }

        public bool isLiked(string user ,int commentID)
        {
            var like 
                = _context.CommentLikes.Any(l => l.UserId == user && l.CommentId == commentID);
            return like;
        }
    }
}
